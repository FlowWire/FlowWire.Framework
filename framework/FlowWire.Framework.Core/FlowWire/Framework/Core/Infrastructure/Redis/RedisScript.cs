using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace FlowWire.Framework.Core.Infrastructure.Redis;

internal sealed class RedisScript(string script)
{
    private readonly string _script = script;
    private readonly byte[] _hash = SHA1.HashData(Encoding.UTF8.GetBytes(script));

    /// <summary>
    /// Executes the Lua script against the specified Redis database with the given keys and values.
    /// </summary>
    public ValueTask<RedisResult> ExecuteAsync(IDatabase db, RedisKey[] keys, RedisValue[] values)
    {
        return ExecuteInternalAsync(db, keys, values);
    }

    /// <summary>
    /// Executes the Lua script against the specified Redis database with key and no values.
    /// </summary>
    public ValueTask<RedisResult> ExecuteAsync(IDatabase db, RedisKey key)
    {
        return ExecuteInternalAsync(db, [key], []);
    }

    /// <summary>
    /// Executes the Lua script against the specified Redis database with a single key and value.
    /// </summary>
    public ValueTask<RedisResult> ExecuteAsync(IDatabase db, RedisKey key, RedisValue value)
    {
        // We have to create arrays for the underlying API, but we isolate it here.
        // (If SE.Redis adds Span support in the future, this becomes truly zero-alloc)
        return ExecuteInternalAsync(db, [key], [value]);
    }

    private async ValueTask<RedisResult> ExecuteInternalAsync(
        IDatabase db,
        RedisKey[] keys,
        RedisValue[] values)
    {
        try
        {
            return await db.ScriptEvaluateAsync(_hash, keys, values).ConfigureAwait(false);
        }
        catch (RedisServerException ex) when (ex.Message.StartsWith("NOSCRIPT"))
        {
            return await db.ScriptEvaluateAsync(_script, keys, values).ConfigureAwait(false);
        }
    }
}