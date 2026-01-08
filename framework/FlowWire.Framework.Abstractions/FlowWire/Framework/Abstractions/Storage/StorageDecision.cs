namespace FlowWire.Framework.Abstractions.Storage;

public readonly struct StorageDecision
{
    public bool IsInline { get; }
    public Type? BackendType { get; }

    private StorageDecision(bool isInline, Type? backendType)
    {
        IsInline = isInline;
        BackendType = backendType;
    }

    public static StorageDecision Inline()
    {
        return new(true, null);
    }

    public static StorageDecision StoreIn<T>() where T : IStorage
    {
        return new(false, typeof(T));
    }
}