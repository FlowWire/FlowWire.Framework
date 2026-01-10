using System.Buffers;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace FlowWire.Framework.Core.Storage;

public class FlowWireKeyStrategy(IOptions<FlowWireOptions> options) : IKeyStrategy
{
    private readonly string _prefix = options.Value.Connection.KeyPrefix;

    public string GetLockKey(string flowId, char separator)
    {
        return BuildKey(separator, "l", flowId);
    }

    public string GetStateKey(string flowId, char separator)
    {
        return BuildKey(separator, "s", flowId);
    }

    public string GetQueueDlqKey(string group, char separator)
    {
        return BuildKey(separator, "q", group, "dlq");
    }

    public string GetQueueInflightKey(string group, char separator)
    {
        return BuildKey(separator, "q", group, "inflight");
    }

    public string GetQueuePendingKey(string group, char separator)
    {
        return BuildKey(separator, "q", group, "pending");
    }

    private string BuildKey(char separator, string s1, string s2)
    {
        var totalLength = _prefix.Length + 1 + s1.Length + 1 + s2.Length;

        return string.Create(totalLength, (_prefix, separator, s1, s2), static (span, state) =>
        {
            var (prefix, sep, seg1, seg2) = state;

            prefix.AsSpan().CopyTo(span);
            var pos = prefix.Length;

            span[pos++] = sep;

            seg1.AsSpan().CopyTo(span[pos..]);
            pos += seg1.Length;

            span[pos++] = sep;

            seg2.AsSpan().CopyTo(span[pos..]);
        });
    }

    private string BuildKey(char separator, string s1, string s2, string s3)
    {
        var totalLength = _prefix.Length + 1 + s1.Length + 1 + s2.Length + 1 + s3.Length;

        return string.Create(totalLength, (_prefix, separator, s1, s2, s3), static (span, state) =>
        {
            var (prefix, sep, seg1, seg2, seg3) = state;

            prefix.AsSpan().CopyTo(span);
            var pos = prefix.Length;

            span[pos++] = sep;

            seg1.AsSpan().CopyTo(span[pos..]);
            pos += seg1.Length;

            span[pos++] = sep;

            seg2.AsSpan().CopyTo(span[pos..]);
            pos += seg2.Length;

            span[pos++] = sep;

            seg3.AsSpan().CopyTo(span[pos..]);
        });
    }

    private string BuildKey(char separator, params ReadOnlySpan<string> segments)
    {
        var totalLength = _prefix.Length;
        for (var i = 0; i < segments.Length; i++)
        {
            totalLength += 1 + segments[i].Length;
        }

        const int StackAllocThreshold = 512;
        if (totalLength <= StackAllocThreshold)
        {
            Span<char> span = stackalloc char[totalLength];
            Fill(span, separator, segments);
            return new string(span);
        }
        else
        {
            var rented = ArrayPool<char>.Shared.Rent(totalLength);
            try
            {
                var span = rented.AsSpan(0, totalLength);
                Fill(span, separator, segments);
                return new string(span);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    private void Fill(Span<char> span, char separator, ReadOnlySpan<string> segments)
    {
        _prefix.AsSpan().CopyTo(span);
        var pos = _prefix.Length;

        for (var i = 0; i < segments.Length; i++)
        {
            span[pos++] = separator;
            segments[i].AsSpan().CopyTo(span[pos..]);
            pos += segments[i].Length;
        }
    }
}