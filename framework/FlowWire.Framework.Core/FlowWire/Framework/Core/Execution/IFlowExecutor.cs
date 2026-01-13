using FlowWire.Framework.Abstractions.Model;

namespace FlowWire.Framework.Core.Execution;

public interface IFlowExecutor
{
    ValueTask ExecuteTickAsync(Impulse impulse);
}
