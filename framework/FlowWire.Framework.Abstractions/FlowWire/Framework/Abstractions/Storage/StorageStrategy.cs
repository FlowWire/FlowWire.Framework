namespace FlowWire.Framework.Abstractions.Storage;

public abstract class StorageStrategy
{
    public abstract StorageDecision Decide(object? value, string propertyName);
}
