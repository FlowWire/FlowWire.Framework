namespace FlowWire.Framework.Core.Infrastructure.Redis;

static internal class LuaScripts
{
    /// <summary>
    /// Attempts to acquire a lock and fetch the current fencing token atomically.
    /// </summary>
    public const string AcquireAndLock = @"
        if redis.call('set', KEYS[1], ARGV[1], 'NX', 'PX', ARGV[2]) then
            local val = redis.call('get', KEYS[2])
            if val == false then return '' end
            return val
        else
            return nil
        end";

    /// <summary>
    /// Saves the result and releases the lock only if the lock is held by the requester.
    /// </summary>
    public const string SaveAndRelease = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            redis.call('set', KEYS[2], ARGV[2])
            redis.call('del', KEYS[1])
            return 1
        else
            return 0
        end";

    /// <summary>
    /// Extends the lock expiration only if the lock is held by the requester.
    /// </summary>
    public const string ExtendLock = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('pexpire', KEYS[1], ARGV[2])
        else
            return 0
        end";
}