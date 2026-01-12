using System.Runtime.CompilerServices;
using FlowWire.Framework.Abstractions.Configuration;
using FlowWire.Framework.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace FlowWire.Framework.Core.Storage;

public class FlowWireKeyStrategy(IOptions<FlowWireOptions> options) : IKeyStrategy
{
    private readonly string _prefix = options.Value.Connection.KeyPrefix;

    private const string SegmentQueue = "q";
    private const string SegmentLock = "l";
    private const string SegmentState = "s";
    private const string SegmentFlow = "f";

    /// <summary>
    /// Generates the flow lock key.
    /// </summary>
    public string GetLockKey(string flowId, char separator)
    {
        return BuildKey(separator, SegmentLock, flowId);
    }

    /// <summary>
    /// Generates the flow state key.
    /// </summary>
    public string GetStateKey(string flowId, char separator)
    {
        return BuildKey(separator, SegmentState, flowId);
    }

    /// <summary>
    /// Generates the flow inbox key.
    /// </summary>
    public string GetInboxKey(string flowId, char separator)
    {
        return BuildKey(separator, SegmentFlow, flowId, "inbox");
    }

    /// <summary>
    /// Generates the queue dead-letter queue key.
    /// </summary>
    public string GetQueueDlqKey(string group, char separator)
    {
        return BuildKey(separator, SegmentQueue, group, "dlq");
    }

    /// <summary>
    /// Generates the queue inflight key.
    /// </summary>
    public string GetQueueInflightKey(string group, char separator)
    {
        return BuildKey(separator, SegmentQueue, group, "inflight");
    }

    /// <summary>
    /// Generates the queue pending key.
    /// </summary>
    public string GetQueuePendingKey(string group, char separator)
    {
        return BuildKey(separator, SegmentQueue, group, "pending");
    }

    public string BuildKey(char separator, string s1, string s2)
    {
        var length = _prefix.Length + 1 + s1.Length + 1 + s2.Length;

        return string.Create(length, (_prefix, separator, s1, s2), static (span, state) =>
        {
            var writer = new SpanWriter(span);
            writer.Write(state._prefix);
            writer.Write(state.separator);
            writer.Write(state.s1);
            writer.Write(state.separator);
            writer.Write(state.s2);
        });
    }

    public string BuildKey(char separator, string s1, string s2, string s3)
    {
        var length = _prefix.Length + 1 + s1.Length + 1 + s2.Length + 1 + s3.Length;

        return string.Create(length, (_prefix, separator, s1, s2, s3), static (span, state) =>
        {
            var writer = new SpanWriter(span);
            writer.Write(state._prefix);
            writer.Write(state.separator);
            writer.Write(state.s1);
            writer.Write(state.separator);
            writer.Write(state.s2);
            writer.Write(state.separator);
            writer.Write(state.s3);
        });
    }

    private ref struct SpanWriter
    {
        private Span<char> _span;
        private int _position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SpanWriter(Span<char> span)
        {
            _span = span;
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            value.AsSpan().CopyTo(_span[_position..]);
            _position += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            _span[_position++] = value;
        }
    }
}