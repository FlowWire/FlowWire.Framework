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

    private const char KeySeparator = ':';

    public async ValueTask<Impulse?> DequeueAsync(string group, CancellationToken ct)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var pendingKey = _keyStrategy.GetQueuePendingKey(group, KeySeparator);
        var inflightKey = _keyStrategy.GetQueueInflightKey(group, KeySeparator);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var timeout = 30000; // Default Visibility Timeout 30s

        var result = await _popWorkScript.ExecuteAsync(db,
            keys: [pendingKey, inflightKey],
            values: [now, timeout]
        );

        if (result.IsNull)
        {
            return null;
        }

        var bytes = (byte[])result!;
        return CacheSerializer.Deserialize<Impulse>(bytes);
    }

    public async ValueTask AckAsync(string group, Impulse impulse)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var inflightKey = _keyStrategy.GetQueueInflightKey(group, KeySeparator);
        var bytes = CacheSerializer.Serialize(impulse, SerializerType.MemoryPack);

        await db.SortedSetRemoveAsync(inflightKey, bytes);
    }

    public async ValueTask NackAsync(string group, Impulse impulse, string reason, bool retryable)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);
        var inflightKey = _keyStrategy.GetQueueInflightKey(group, KeySeparator);
        var dlqKey = _keyStrategy.GetQueueDlqKey(group, KeySeparator);

        var bytes = CacheSerializer.Serialize(impulse, SerializerType.MemoryPack);

        await RemoveFromInflightAsync(db, inflightKey, bytes);

        if (retryable && impulse.DeliveryCount < 5)
        {
            var pendingKey = _keyStrategy.GetQueuePendingKey(group, KeySeparator);
            await MoveToPendingAsync(db, pendingKey, bytes);
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