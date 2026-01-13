using System.Collections.Concurrent;
using FlowWire.Framework.Abstractions;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Model;
using FlowWire.Framework.Abstractions.Storage;
using FlowWire.Framework.Core.Infrastructure.Redis;
using FlowWire.Framework.Core.Serialization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FlowWire.Framework.Core.Execution;

public class RedisImpulseQueue(IConnectionMultiplexer redis, IKeyStrategy keyStrategy, IOptions<FlowWireOptions> options) : IImpulseQueue
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IKeyStrategy _keyStrategy = keyStrategy;
    private readonly FlowWireOptions _options = options.Value;
    private readonly RedisScript _popWorkScript = new(LuaScripts.PopWork);
    private readonly RedisScript _popWorkBatchScript = new(LuaScripts.PopWorkBatch);

    private readonly ConcurrentDictionary<string, (string PendingStr, string InflightStr, RedisKey[] Keys)> _queueKeys = new();

    private const char KeySeparator = ':';

    private (string PendingStr, string InflightStr, RedisKey[] Keys) GetCachedQueueKeys(string group)
    {
        return _queueKeys.GetOrAdd(group, g =>
        {
            var p = _keyStrategy.GetQueuePendingKey(g, KeySeparator);
            var i = _keyStrategy.GetQueueInflightKey(g, KeySeparator);
            return (p, i, [p, i]);
        });
    }

    public async ValueTask<Impulse?> DequeueAsync(string group, CancellationToken ct)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var (_, _, Keys) = GetCachedQueueKeys(group);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeout = 30000; // Default Visibility Timeout 30s

        var result = await _popWorkScript.ExecuteAsync(db,
            keys: Keys,
            values: [now, timeout]
        );

        if (result.IsNull)
        {
            return null;
        }

        return CacheSerializer.Deserialize<Impulse>((byte[])result!);
    }

    public async ValueTask<IReadOnlyList<Impulse>> DequeueBatchAsync(string group, int batchSize, CancellationToken ct)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var (_, _, Keys) = GetCachedQueueKeys(group);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeout = 30000; // Default Visibility Timeout 30s

        var result = await _popWorkBatchScript.ExecuteAsync(db,
            keys: Keys,
            values: [now, timeout, batchSize]
        );

        if (result.IsNull)
        {
            return [];
        }

        var results = (RedisValue[]?)result;
        if (results == null || results.Length == 0)
        {
            return [];
        }

        var impulses = new Impulse[results.Length];
        var count = 0;

        foreach (var val in results)
        {
            if (val.IsNull)
            {
                continue;
            }

            var impulse = CacheSerializer.Deserialize<Impulse>((ReadOnlyMemory<byte>)(byte[])val!);
            if (impulse != null)
            {
                impulses[count++] = impulse;
            }
        }

        return count == impulses.Length ? impulses : impulses.AsSpan(0, count).ToArray();
    }

    public async ValueTask AckAsync(string group, Impulse impulse)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var (_, InflightStr, _) = GetCachedQueueKeys(group);
        var bytes = CacheSerializer.Serialize(impulse, SerializerType.MemoryPack);

        await db.SortedSetRemoveAsync(InflightStr, bytes);
    }

    public async ValueTask AckBatchAsync(string group, IReadOnlyList<Impulse> impulses)
    {
        if (impulses.Count == 0)
        {
            return;
        }

        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var (_, InflightStr, _) = GetCachedQueueKeys(group);

        var valuesArray = new RedisValue[impulses.Count];
        for (var i = 0; i < impulses.Count; i++)
        {
            valuesArray[i] = CacheSerializer.Serialize(impulses[i], SerializerType.MemoryPack);
        }
        await db.SortedSetRemoveAsync(InflightStr, valuesArray);
    }

    public async ValueTask NackAsync(string group, Impulse impulse, string reason, bool retryable)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var (PendingStr, InflightStr, _) = GetCachedQueueKeys(group);
        var dlqKey = _keyStrategy.GetQueueDlqKey(group, KeySeparator); // DLQ is rare, can stay uncached or cache if we expand the tuple

        var bytes = CacheSerializer.Serialize(impulse, SerializerType.MemoryPack);

        await RemoveFromInflightAsync(db, InflightStr, bytes);

        if (retryable && impulse.DeliveryCount < 5)
        {
            await MoveToPendingAsync(db, PendingStr, bytes);
        }
        else
        {
            await MoveToDlqAsync(db, dlqKey, bytes);
        }
    }

    private async static ValueTask RemoveFromInflightAsync(IDatabase db, string inflightKey, byte[] itemBytes)
    {
        await db.SortedSetRemoveAsync(inflightKey, itemBytes);
    }

    private async static ValueTask MoveToDlqAsync(IDatabase db, string dlqKey, byte[] itemBytes)
    {
        await db.ListRightPushAsync(dlqKey, itemBytes);
    }

    private async static ValueTask MoveToPendingAsync(IDatabase db, string pendingKey, byte[] itemBytes)
    {
        await db.ListRightPushAsync(pendingKey, itemBytes);
    }
}