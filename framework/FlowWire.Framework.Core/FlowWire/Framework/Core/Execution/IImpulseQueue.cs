using FlowWire.Framework.Abstractions.Model;

namespace FlowWire.Framework.Core.Execution;

public interface IImpulseQueue
{
    /// <summary>
    /// Attempts to pop the next available impulse from the work queue.
    /// This moves the item to the In-Flight set for reliability.
    /// </summary>
    ValueTask<Impulse?> DequeueAsync(string group, CancellationToken ct);

    /// <summary>
    /// Attempts to pop multiple impulses from the work queue.
    /// </summary>
    ValueTask<IReadOnlyList<Impulse>> DequeueBatchAsync(string group, int batchSize, CancellationToken ct);

    /// <summary>
    /// Acknowledges successful processing, removing the item from In-Flight.
    /// </summary>
    ValueTask AckAsync(string group, Impulse impulse);

    /// <summary>
    /// Acknowledges multiple successful processings, removing the items from In-Flight.
    /// </summary>
    ValueTask AckBatchAsync(string group, IReadOnlyList<Impulse> impulses);

    /// <summary>
    /// Negative Acknowledgement. Releases the item back to the queue (or DLQ) depending on policy.
    /// </summary>
    ValueTask NackAsync(string group, Impulse impulse, string reason, bool retryable);
}
