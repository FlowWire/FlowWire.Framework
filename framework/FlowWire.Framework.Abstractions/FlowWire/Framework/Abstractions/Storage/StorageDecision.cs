namespace FlowWire.Framework.Abstractions.Storage;

public readonly struct StorageDecision
{
    public bool IsInline { get; }
    public string? BackendName { get; }

    private StorageDecision(bool isInline, string? backendName)
    {
        IsInline = isInline;
        BackendName = backendName;
    }

    public static StorageDecision Inline()
    {
        return new(true, null);
    }

    public static StorageDecision StoreIn(string backendName)
    {
        return new(false, backendName);
    }
}