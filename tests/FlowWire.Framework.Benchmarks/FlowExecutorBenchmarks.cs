using BenchmarkDotNet.Attributes;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Model;
using FlowWire.Framework.Core.Execution;
using FlowWire.Framework.Core.Registry;
using FlowWire.Framework.Core.Storage;
using FlowWire.Framework.Core.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Testcontainers.Redis;
using MemoryPack;
using Microsoft.VSDiagnostics;

namespace FlowWire.Framework.Benchmarks;

[CPUUsageDiagnoser]
[MemoryDiagnoser]
public class FlowExecutorBenchmarks
{
    private RedisContainer _container = null!;
    private IConnectionMultiplexer _redis = null!;
    private FlowExecutor _executor = null!;
    private Impulse _memoryImpulse = null!;
    private Impulse _circuitImpulse = null!;
    private IDatabase _db = null!;
    
    private string _memLockKey = null!;
    private string _memStateKey = null!;
    private string _circLockKey = null!;
    private string _circStateKey = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _container = new RedisBuilder("valkey/valkey:latest").Build();
        _container.StartAsync().GetAwaiter().GetResult();

        var config = ConfigurationOptions.Parse(_container.GetConnectionString());
        _redis = ConnectionMultiplexer.Connect(config);
        _db = _redis.GetDatabase();

        var options = new FlowWireOptions();
        options.Execution.LockTimeout = TimeSpan.FromSeconds(30);
        options.Connection.DatabaseIndex = 0;

        var registry = new FlowTypeRegistry(NullLogger<FlowTypeRegistry>.Instance);
        var keyStrategy = new FlowWireKeyStrategy(Options.Create(options));
        
        _executor = new FlowExecutor(
            _redis,
            Options.Create(options),
            registry,
            keyStrategy,
            new BenchServiceProvider(),
            NullLogger<FlowExecutor>.Instance
        );

        _memoryImpulse = new Impulse
        {
            FlowId = "mem-flow-1",
            FlowType = nameof(BenchmarkMemoryFlow),
            ImpulseName = "BenchmarkSignal"
        };

        _circuitImpulse = new Impulse
        {
            FlowId = "circ-flow-1",
            FlowType = nameof(BenchmarkCircuitFlow),
            ImpulseName = "BenchmarkSignal"
        };

        _memLockKey = keyStrategy.GetLockKey(_memoryImpulse.FlowId, ':');
        _memStateKey = keyStrategy.GetStateKey(_memoryImpulse.FlowId, ':');
        _circLockKey = keyStrategy.GetLockKey(_circuitImpulse.FlowId, ':');
        _circStateKey = keyStrategy.GetStateKey(_circuitImpulse.FlowId, ':');
        
        // Warmup
        _executor.ExecuteTickAsync(_memoryImpulse).AsTask().GetAwaiter().GetResult();
        _executor.ExecuteTickAsync(_circuitImpulse).AsTask().GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _redis?.Dispose();
        _container?.DisposeAsync().AsTask().Wait();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _db.KeyDelete(_memLockKey);
        _db.KeyDelete(_memStateKey);
        _db.KeyDelete(_circLockKey);
        _db.KeyDelete(_circStateKey);
        
        var state = new BenchmarkState { Counter = 42 };
        var bytes = CacheSerializer.Serialize(state, SerializerType.MemoryPack);
        
        _db.StringSet(_memStateKey, bytes);
        _db.StringSet(_circStateKey, bytes);
    }

    [Benchmark]
    public async Task ExecuteTick_MemoryFlow()
    {
        await _executor.ExecuteTickAsync(_memoryImpulse);
    }

    [Benchmark]
    public async Task ExecuteTick_CircuitFlow()
    {
        await _executor.ExecuteTickAsync(_circuitImpulse);
    }
}

public class BenchServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(BenchmarkMemoryFlow)) return new BenchmarkMemoryFlow();
        if (serviceType == typeof(BenchmarkCircuitFlow)) return new BenchmarkCircuitFlow();
        return null;
    }
}

[MemoryPackable]
public partial class BenchmarkState
{
    public int Counter { get; set; }
}

[Flow(Mode = FlowMode.Memory)]
[GeneratedFlowConfiguration(typeof(BenchmarkState))]
public class BenchmarkMemoryFlow : IFlow
{
    private BenchmarkState _state = new();

    public void SetState(object state) => _state = (BenchmarkState)state;
    public object GetState() => _state;
    public void SetContext(IFlowContext context) { }
    public void DispatchSignal(string signalName, object? arg) { }
    public object? DispatchQuery(string queryName, object? arg) => null;
    public FlowCommand Execute() => Command.Finish();
    public void Reset() { _state = new(); }
}

[Flow(Mode = FlowMode.Circuit)]
[GeneratedFlowConfiguration(typeof(BenchmarkState))]
public class BenchmarkCircuitFlow : IFlow
{
    private BenchmarkState _state = new();

    public void SetState(object state) => _state = (BenchmarkState)state;
    public object GetState() => _state;
    public void SetContext(IFlowContext context) { }
    public void DispatchSignal(string signalName, object? arg) { }
    public object? DispatchQuery(string queryName, object? arg) => null;
    
    public FlowCommand Execute()
    {
        // Simulate a small state transition
        _state.Counter++;
        return Command.Finish();
    }

    public void Reset() { _state = new(); }
}
