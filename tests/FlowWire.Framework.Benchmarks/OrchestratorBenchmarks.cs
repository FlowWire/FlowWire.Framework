using BenchmarkDotNet.Attributes;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Model;
using FlowWire.Framework.Core.Execution;
using FlowWire.Framework.Core.Serialization;
using FlowWire.Framework.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VSDiagnostics;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace FlowWire.Framework.Benchmarks;

[CPUUsageDiagnoser]
[MemoryDiagnoser]
public class OrchestratorBenchmarks
{
    [Params(10_000)]
    public int ImpulseCount = 0;
    private List<Impulse> _impulses = null!;

    [Params(Contention.None, Contention.High)]
    public Contention Scenario;

    public enum Contention { None, High }

    private IImpulseQueue _queue = null!;
    private MockFlowExecutor _executor = null!;
    
    private OrchestratorBackgroundServiceNaive _naive = null!;
    private FlowWireOptions _naiveOptions = null!;

    private OrchestratorBackgroundService _pipelined = null!;
    private FlowWireOptions _pipelinedOptions = null!;

    private RedisContainer _valkeyContainer = null!;
    private IConnectionMultiplexer _redis = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var cpuCores = Environment.ProcessorCount;
        var ioThreads = cpuCores * 4;
        
        _naiveOptions = new FlowWireOptions
        {
            Orchestrator = new OrchestratorOptions
            {
                Concurrency = ioThreads,
                MaxInboxBatchSize = 1, // Naive does not use it
                PollInterval = TimeSpan.FromMilliseconds(50)
            }
        };
        _naiveOptions.Connection.KeyPrefix = "naive";

        _pipelinedOptions = new FlowWireOptions
        {
            Orchestrator = new OrchestratorOptions
            {
                Concurrency = cpuCores,
                MaxInboxBatchSize = 100,
                PollInterval = TimeSpan.FromMilliseconds(50)
            }
        };
        _pipelinedOptions.Connection.KeyPrefix = "pipe";

        _valkeyContainer = new RedisBuilder("valkey/valkey:latest")
            .Build();

        _valkeyContainer.StartAsync().GetAwaiter().GetResult();
        var config = ConfigurationOptions.Parse(_valkeyContainer.GetConnectionString());
        config.AllowAdmin = true;
        _redis = ConnectionMultiplexer.Connect(config);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _redis?.Dispose();
        _valkeyContainer?.DisposeAsync().AsTask().Wait();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _impulses = new List<Impulse>(ImpulseCount);
        for (var i = 0; i < ImpulseCount; i++)
        {
            var flowId = Scenario == Contention.None ? i.ToString() : "HOT-FLOW";

            _impulses.Add(new Impulse
            {
                Id = i.ToString(),
                FlowId = flowId,
                FlowType = "TestFlow",
                ImpulseName = "TestSignal"
            });
        }

        _executor = new MockFlowExecutor();

        // Flush database to ensure clean state
        var endpoints = _redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
             _redis.GetServer(endpoint).FlushDatabase();
        }

        // We prepare queues for both scenarios (simplest approach)
        PopulateQueue(_naiveOptions, _impulses);
        PopulateQueue(_pipelinedOptions, _impulses);
    }
    
    private void PopulateQueue(FlowWireOptions options, List<Impulse> impulses)
    {
        var keyStrategy = new FlowWireKeyStrategy(Options.Create(options));
        var db = _redis.GetDatabase(options.Connection.DatabaseIndex);
        var pendingKey = keyStrategy.GetQueuePendingKey("default", ':');
        
        var values = new RedisValue[impulses.Count];
        for (var i = 0; i < impulses.Count; i++)
        {
             // Serialize using the same serializer as RedisImpulseQueue
             values[i] = CacheSerializer.Serialize(impulses[i], SerializerType.MemoryPack);
        }
        
        db.ListRightPushAsync(pendingKey, values).Wait();
    }
    
    [Benchmark(Baseline = true)]
    public async Task Naive()
    {
        // Setup Queue for this run
        var keyStrategy = new FlowWireKeyStrategy(Options.Create(_naiveOptions));
        _queue = new RedisImpulseQueue(_redis, keyStrategy, Options.Create(_naiveOptions));

        _naive = new OrchestratorBackgroundServiceNaive(
            _queue,
            _executor,
            Options.Create(_naiveOptions),
            NullLogger<OrchestratorBackgroundServiceNaive>.Instance);


        using var cts = new CancellationTokenSource();
        var task = _naive.StartAsync(cts.Token);
        
        // Wait until processed
        await WaitForProcessingAsync(_queue, cts.Token);
        
        cts.Cancel();
        await _naive.StopAsync(CancellationToken.None);
    }
    

    [Benchmark]
    public async Task Pipelined()
    {
        // Setup Queue for this run
        var keyStrategy = new FlowWireKeyStrategy(Options.Create(_pipelinedOptions));
        _queue = new RedisImpulseQueue(_redis, keyStrategy, Options.Create(_pipelinedOptions));

        _pipelined = new OrchestratorBackgroundService(
            _queue,
            _executor,
            Options.Create(_pipelinedOptions),
            NullLogger<OrchestratorBackgroundService>.Instance);

        using var cts = new CancellationTokenSource();
        var task = _pipelined.StartAsync(cts.Token);
        
        // Wait until processed
        await WaitForProcessingAsync(_queue, cts.Token);
        
        cts.Cancel();
        await _pipelined.StopAsync(CancellationToken.None);
    }

    private async Task WaitForProcessingAsync(IImpulseQueue queue, CancellationToken ct)
    {
        // We need to wait until the queue is empty AND inflight is empty (or Acked)
        // Since we are checking consumption speed, we can poll the pending/inflight counts.
        
        // Actually, the original benchmark awaited `_queue.AllProcessed`.
        // MockImpulseQueue had an internal counter.
        // With Redis, we need to query Redis.
        
        // Which key?
        // We know the options from the context? No, passed via queue?
        // The queue instance has options but they are private.
        // Use the current options of the benchmark method... which one?
        // We can pass the options to this helper.
        
        // Wait! Naive() and Pipelined() sets `_queue`, so we know which one it is.
        // We need to know which Options were used to create `_queue` to know the keys.
        
        // I will add options parameter to WaitForProcessingAsync
        // But better: Check executor count? 
        // MockFlowExecutor can count executed impulses!
        
        while (_executor.ExecutedCount < ImpulseCount)
        {
             await Task.Delay(10, ct);
        }
    }
}
