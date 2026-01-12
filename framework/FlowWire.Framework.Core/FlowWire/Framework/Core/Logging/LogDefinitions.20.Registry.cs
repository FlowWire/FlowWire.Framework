using Microsoft.Extensions.Logging;

namespace FlowWire.Framework.Core.Logging;

static internal partial class LogDefinitions
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Registered Flow Type: {flowType}")]
    public static partial void LogRegisteredFlowType(this ILogger logger, string flowType);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Failed to register Flow Type by simple name: {flowType}. Name already in use.")]
    public static partial void LogFailedRegisterFlowTypeBySimpleName(this ILogger logger, string flowType);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "Failed to register Flow Type by full name: {flowType}. Name already in use.")]
    public static partial void LogFailedRegisterFlowTypeByFullName(this ILogger logger, string flowType);
}
