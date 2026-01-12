using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Locking;
using FlowWire.Framework.Abstractions.Storage;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace FlowWire.Framework.Core.Infrastructure.Redis;

public class RedisLockBackend(IConnectionMultiplexer redis, IKeyStrategy keyStrategy, IOptions<FlowWireOptions> options) : IStorage
{
    private const char KeySeparator = ':';

    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IKeyStrategy _keyStrategy = keyStrategy;
    private readonly FlowWireOptions _options = options.Value;

    private readonly RedisScript _acquireScript = new(LuaScripts.AcquireAndLoad);
    private readonly RedisScript _saveScript = new(LuaScripts.SaveAndRelease);
    private readonly RedisScript _heartbeatScript = new(LuaScripts.ExtendLock);

    public async Task<string> WriteAsync(string blobName, byte[] data, CancellationToken ct = default)
    {
        throw new NotSupportedException($"Direct write is not supported in {nameof(RedisLockBackend)}. Use {nameof(TryAcquireAndLoadAsync)} and {nameof(SaveAndReleaseAsync)} instead.");
    }

    public async Task<byte[]> ReadAsync(string referenceKey, CancellationToken ct = default)
    {
        throw new NotSupportedException($"Direct read is not supported in {nameof(RedisLockBackend)}. Use {nameof(TryAcquireAndLoadAsync)} instead.");
    }

    public async ValueTask<(FlowLease, byte[])> TryAcquireAndLoadAsync(string flowId, string token)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);

        var lockKey = _keyStrategy.GetLockKey(flowId, KeySeparator);
        var stateKey = _keyStrategy.GetStateKey(flowId, KeySeparator);

        var timeoutMs = (long)_options.Execution.LockTimeout.TotalMilliseconds;

        var result = await _acquireScript.ExecuteAsync(db,
            keys: [lockKey, stateKey],
            values: [token, timeoutMs]
        );

        if (IsLocked(result))
        {
            return (FlowLease.Failed(flowId), Array.Empty<byte>());
        }

        return (new FlowLease(flowId, token, true), (byte[]?)result ?? []);
    }

    public async ValueTask<bool> SaveAndReleaseAsync(string flowId, string token, byte[] data)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);

        var lockKey = _keyStrategy.GetLockKey(flowId, KeySeparator);
        var stateKey = _keyStrategy.GetStateKey(flowId, KeySeparator);

        var result = await _saveScript.ExecuteAsync(db,
            keys: [lockKey, stateKey],
            values: [token, data]
        );

        return (int)result == 1;
    }

    public async ValueTask<bool> HeartbeatAsync(string flowId, string token)
    {
        var db = _redis.GetDatabase(_options.Connection.DatabaseIndex);

        var lockKey = _keyStrategy.GetLockKey(flowId, KeySeparator);
        var timeoutMs = (long)_options.Execution.LockTimeout.TotalMilliseconds;

        var result = await _heartbeatScript.ExecuteAsync(db,
            keys: [lockKey],
            values: [token, timeoutMs]
        );

        return (int)result == 1;
    }

    private static bool IsLocked(RedisResult? result)
    {
        return result is not null && result.IsNull;
    }
}