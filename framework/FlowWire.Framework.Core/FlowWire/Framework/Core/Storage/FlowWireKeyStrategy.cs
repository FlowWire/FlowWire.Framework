using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace FlowWire.Framework.Core.Storage;

public class FlowWireKeyStrategy(IOptions<FlowWireOptions> options) : IKeyStrategy
{
    private readonly string _prefix = options.Value.Connection.KeyPrefix;

    public string GetLockKey(string flowId, char separator)
    {
        return BuildKey(flowId, "lock", separator);
    }

    public string GetStateKey(string flowId, char separator)
    {
        return BuildKey(flowId, "state", separator);
    }

    private string BuildKey(string flowId, string segment, char separator)
    {
        var totalLength = _prefix.Length + 1 + segment.Length + 1 + flowId.Length;

        return string.Create(totalLength, (_prefix, separator, segment, flowId), static (span, state) =>
        {
            var (prefix, sep, seg, id) = state;

            prefix.AsSpan().CopyTo(span);
            var pos = prefix.Length;

            span[pos++] = sep;

            seg.AsSpan().CopyTo(span[pos..]);
            pos += seg.Length;

            span[pos++] = sep;

            id.AsSpan().CopyTo(span[pos..]);
        });
    }
}