using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Model;
using FlowWire.Framework.Abstractions.Storage;
using FlowWire.Framework.Core.Helpers;
using FlowWire.Framework.Core.Infrastructure.Redis;
using FlowWire.Framework.Core.Logging;
using FlowWire.Framework.Core.Registry;
using FlowWire.Framework.Core.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FlowWire.Framework.Core.Execution;

public class FlowExecutor : IFlowExecutor
{
    private readonly IConnectionMultiplexer _redis;
    private readonly FlowWireOptions _options;
    private readonly FlowTypeRegistry _registry;
    private readonly IKeyStrategy _keyStrategy;
    private readonly ILogger<FlowExecutor> _logger;
    private readonly RedisScript _acquireScript;
    private readonly RedisScript _saveScript;
    private readonly RedisScript _releaseScript;

    // Pools
    private readonly FrozenDictionary<Type, ObjectPool<IFlow>> _flowPools;
    private readonly ObjectPool<PooledFlowContext> _contextPool;

    // Key Cache (Simple LRU would be better for massive unique Flow IDs, but this covers hotspots)
    private readonly ConcurrentDictionary<string, (RedisKey Lock, RedisKey State, RedisKey Inbox)> _keyCache = new();

    public FlowExecutor(
        IConnectionMultiplexer redis,
        IOptions<FlowWireOptions> options,
        FlowTypeRegistry registry,
        IKeyStrategy keyStrategy,
        IServiceProvider serviceProvider,
        ILogger<FlowExecutor> logger)
    {
        _redis = redis;
        _options = options.Value;
        _registry = registry;
        _keyStrategy = keyStrategy;
        _logger = logger;
        _acquireScript = new(LuaScripts.AcquireAndLoad);
        _saveScript = new(LuaScripts.SaveAndRelease);
        _releaseScript = new(LuaScripts.Release);

        // Initialize Context Pool
        var contextProvider = new DefaultObjectPoolProvider();
        _contextPool = contextProvider.Create(new PooledFlowContextPolicy(serviceProvider));

        Dictionary<Type, ObjectPool<IFlow>> flowPools = [];
        foreach (var type in _registry.GetAllFlowTypes())
        {
            var policy = new FlowPoolingPolicy(type, serviceProvider);
            flowPools[type] = contextProvider.Create(policy);
        }
        _flowPools = flowPools.ToFrozenDictionary();
    }

    public async ValueTask ExecuteTickAsync(Impulse impulse)
    {
        var meta = _registry.GetFlowMetadata(impulse.FlowType);
        if (meta?.FlowType is null)
        {
            return;
        }

        var sw = ValueStopwatch.StartNew();

        try
        {
            var (lockKey, stateKey, inboxKey) = GetKeys(impulse.FlowId);
            var fenceToken = GenerateFenceToken();
            var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);

            var loadResult = await AcquireLockAndLoadDataAsync(db, lockKey, stateKey, inboxKey, fenceToken);

            if (loadResult.IsNull)
            {
                _logger.LogLockContention(impulse.FlowId);
                return;
            }

            var (stateBytes, inboxItems) = UnpackLoadResult(loadResult);

            if (!IsEnergized(meta, stateBytes, impulse))
            {
                _logger.LogFlowNotEnergized(impulse.FlowId, impulse.ImpulseName);
                await ReleaseLockAsync(db, lockKey, fenceToken);
                return;
            }

            var flow = _flowPools[meta.FlowType].Get();
            var context = _contextPool.Get();

            try
            {
                InitializeFlow(flow, meta, context, stateBytes, inboxItems, impulse);

                var command = flow.Execute();

                var success = await CommitFlowStateAsync(db, flow, meta, lockKey, stateKey, fenceToken);

                if (!success)
                {
                    _logger.LogConcurrencyFault(impulse.FlowId);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Information)) // Perf: Avoid ToString() if not needed
                    {
                        _logger.LogFlowTick(impulse.FlowId, meta.FlowType.Name, sw.GetElapsedMilliseconds(), command.ToString()!);
                    }
                }
            }
            finally
            {
                ReturnToPool(flow, meta, context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogFlowFailure(impulse.FlowId, ex);
            throw;
        }
    }

    private static RedisValue GenerateFenceToken()
    {
        return Stopwatch.GetTimestamp();
    }

    private (RedisKey LockKey, RedisKey StateKey, RedisKey InboxKey) GetKeys(string flowId)
    {
        if (_keyCache.TryGetValue(flowId, out var cached))
        {
            return cached;
        }

        const char KeySeparator = ':';
        var keys = (
            (RedisKey)_keyStrategy.GetLockKey(flowId, KeySeparator),
            (RedisKey)_keyStrategy.GetStateKey(flowId, KeySeparator),
            (RedisKey)_keyStrategy.GetInboxKey(flowId, KeySeparator)
        );

        if (_keyCache.Count < 10000)
        {
            _keyCache.TryAdd(flowId, keys);
        }

        return keys;
    }

    private async Task<RedisResult> AcquireLockAndLoadDataAsync(IDatabase db, RedisKey lockKey, RedisKey stateKey, RedisKey inboxKey, RedisValue fenceToken)
    {
        var ttl = (long)_options.Execution.LockTimeout.TotalMilliseconds;
        return await _acquireScript.ExecuteAsync(db,
            lockKey, stateKey, inboxKey,
            fenceToken, ttl, _options.Orchestrator.MaxInboxBatchSize
        );
    }

    private async Task ReleaseLockAsync(IDatabase db, RedisKey lockKey, RedisValue fenceToken)
    {
        await _releaseScript.ExecuteAsync(db, lockKey, fenceToken);
    }

    private static (byte[]? StateBytes, RedisResult[]? InboxItems) UnpackLoadResult(RedisResult loadResult)
    {
        var results = (RedisResult[])loadResult!;
        return (
            (byte[]?)results[0],
            (RedisResult[]?)results[1]
        );
    }

    private void InitializeFlow(IFlow flow, FlowMetadata meta, PooledFlowContext context, byte[]? stateBytes, RedisResult[]? inboxItems, Impulse impulse)
    {
        context.Initialize(impulse.FlowId, DateTimeOffset.UtcNow);

        if (stateBytes != null && stateBytes.Length > 0)
        {
            var state = CacheSerializer.Deserialize(stateBytes, meta.StateType);
            flow.SetState(state!);
        }

        flow.SetContext(context);

        if (inboxItems != null && inboxItems.Length > 0)
        {
            foreach (var item in inboxItems)
            {
                var inboxBytes = (byte[])item!;
                var inboxMsg = CacheSerializer.Deserialize<Impulse>(inboxBytes);
                if (inboxMsg != null)
                {
                    flow.DispatchSignal(inboxMsg.ImpulseName, inboxMsg.Payload);
                }
            }
        }

        flow.DispatchSignal(impulse.ImpulseName, impulse.Payload);
    }

    private async Task<bool> CommitFlowStateAsync(IDatabase db, IFlow flow, FlowMetadata meta, RedisKey lockKey, RedisKey stateKey, RedisValue fenceToken)
    {
        var newState = flow.GetState();
        var newStateBytes = CacheSerializer.Serialize(newState, meta.StateType, _options.Serialization.StateSerializer);

        var saveResult = await _saveScript.ExecuteAsync(db,
            lockKey, stateKey,
            fenceToken, newStateBytes
        );

        return (int)saveResult != 0;
    }

    private void ReturnToPool(IFlow flow, FlowMetadata meta, PooledFlowContext context)
    {
        flow.Reset();
        _flowPools[meta.FlowType].Return(flow);

        context.Initialize(string.Empty, default);
        _contextPool.Return(context);
    }

    private static bool IsEnergized(FlowMetadata meta, byte[]? stateBytes, Impulse impulse)
    {
        if (stateBytes is not null && stateBytes.Length > 0)
        {
            return true;
        }

        if (meta.Mode != FlowMode.Circuit)
        {
            return true;
        }

        return meta.EnergizeImpulses.Contains(impulse.ImpulseName);
    }
}
