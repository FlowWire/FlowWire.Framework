namespace FlowWire.Framework.Core.Infrastructure.Redis;

static internal class LuaScripts
{
    /// <summary>
    /// Attempts to acquire a lock and fetch the current fencing token atomically.
    /// KEYS[1]: Lock Key
    /// KEYS[2]: State Key
    /// KEYS[3]: Inbox Key
    /// ARGV[1]: Fencing Token (New)
    /// ARGV[2]: Lock TTL (ms)
    /// ARGV[3]: Max Inbox Batch Size
    /// </summary>
    public const string AcquireAndLoad = @"
        if redis.call('set', KEYS[1], ARGV[1], 'NX', 'PX', ARGV[2]) then
            local state = redis.call('get', KEYS[2])
            local inbox = redis.call('lpop', KEYS[3], ARGV[3])
            return { state, inbox }
        else
            return nil
        end";

    /// <summary>
    /// Saves the result and releases the lock only if the lock is held by the requester.
    /// KEYS[1]: Lock Key
    /// KEYS[2]: State Key
    /// ARGV[1]: Fencing Token
    /// ARGV[2]: New Binary State
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
    /// KEYS[1]: Lock Key
    /// ARGV[1]: Fencing Token
    /// ARGV[2]: New TTL (ms)
    /// </summary>
    public const string ExtendLock = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('pexpire', KEYS[1], ARGV[2])
        else
            return 0
        end";

    /// <summary>
    /// Atomically pops an item from Pending and moves it to InFlight with a visibility timeout score.
    /// KEYS[1]: Pending List
    /// KEYS[2]: In-Flight ZSet
    /// ARGV[1]: Current Time (Unix MS)
    /// ARGV[2]: Visibility Timeout (ms)
    /// </summary>
    public const string PopWork = @"
        local msg = redis.call('rpop', KEYS[1])
        if msg then
            local expiry = tonumber(ARGV[1]) + tonumber(ARGV[2])
            redis.call('zadd', KEYS[2], expiry, msg)
            return msg
        end
        return nil";
}