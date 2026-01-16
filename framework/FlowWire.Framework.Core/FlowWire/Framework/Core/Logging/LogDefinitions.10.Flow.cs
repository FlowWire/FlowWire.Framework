using Microsoft.Extensions.Logging;

namespace FlowWire.Framework.Core.Logging;

static internal partial class LogDefinitions
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Flow {flowId} ({flowType}) completed tick in {duration}ms. Result: {result}")]
    public static partial void LogFlowTick(this ILogger logger, string flowId, string flowType, long duration, string result);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Flow {flowId} failed to acquire lock. Retrying.")]
    public static partial void LogLockContention(this ILogger logger, string flowId);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Error, Message = "Flow {flowId} failed with exception.")]
    public static partial void LogFlowFailure(this ILogger logger, string flowId, Exception ex);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "Optimistic concurrency fault for Flow {flowId}. Discarding result.")]
    public static partial void LogConcurrencyFault(this ILogger logger, string flowId);
    
    [LoggerMessage(EventId = 1005, Level = LogLevel.Warning, Message = "Flow {FlowId} is Cold (Not Existent) and Mode is Circuit. Impulse '{Impulse}' is not marked as [Energizes]. Execution rejected.")]
    public static partial void LogFlowNotEnergized(this ILogger logger, string flowId, string impulse);
}
