namespace FlowWire.Framework.Abstractions.Storage.Strategies;

public class InlineStrategy : StorageStrategy
{
    public override StorageDecision Decide(object? value, string propertyName)
    {
        return StorageDecision.Inline();
    }
}
