namespace FlowWire.Framework.Abstractions.Storage.Strategies;

public sealed class AutoStrategy : StorageStrategy
{
    public override StorageDecision Decide(object? value, string propertyName)
    {
        if (value is byte[] b && b.Length > 4096)
        {
            return StorageDecision.StoreIn("Default");
        }
        return StorageDecision.Inline();
    }
}
