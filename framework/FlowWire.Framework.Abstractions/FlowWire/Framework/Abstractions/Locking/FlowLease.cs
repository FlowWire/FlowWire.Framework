namespace FlowWire.Framework.Abstractions.Locking;

public readonly struct FlowLease(string flowId, string token, bool success)
{
    /// The ID of the flow this lease belongs to.
    /// </summary>
    public string FlowId { get; } = flowId;

    /// <summary>
    /// The unique Fencing Token used to validate ownership.
    /// If Success is false, this may be null/empty.
    /// </summary>
    public string Token { get; } = token;

    /// <summary>
    /// True if the lock was successfully acquired.
    /// False if the flow is currently locked by another worker.
    /// </summary>
    public bool Success { get; } = success;

    public static FlowLease Failed(string flowId)
    {
        return new(flowId, string.Empty, false);
    }
}