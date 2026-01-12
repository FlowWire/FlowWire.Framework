using Microsoft.Extensions.Logging;

namespace FlowWire.Framework.Core.Logging;

static internal partial class LogDefinitions
{
    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Orchestrator started with {concurrency} workers.")]
    public static partial void LogOrchestratorStarted(this ILogger logger, int concurrency);

    [LoggerMessage(EventId = 3002, Level = LogLevel.Warning, Message = "Impulse {impulseId} failed processing: {reason}")]
    public static partial void LogImpulseProcessingFailed(this ILogger logger, string impulseId, string reason);

    [LoggerMessage(EventId = 3003, Level = LogLevel.Error, Message = "Fatal error in Worker {workerId} loop.")]
    public static partial void LogFatalOrchestratorError(this ILogger logger, int workerId, Exception ex);
}
