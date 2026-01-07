namespace FlowWire.Framework.Abstractions;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class QueryAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
    public bool IsCached { get; set; } = false;

    public CacheFormat Format { get; set; } = CacheFormat.MemoryPack;
}
