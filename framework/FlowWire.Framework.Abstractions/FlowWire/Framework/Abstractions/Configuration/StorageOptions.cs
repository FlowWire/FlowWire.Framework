using FlowWire.Framework.Abstractions.Storage;

namespace FlowWire.Framework.Abstractions.Configuration;


public class StorageOptions
{
    /// <summary>
    /// If true, state saved in Redis (Inline) will be compressed using Brotli.
    /// </summary>
    public bool CompressInlineState { get; set; } = true;

    // Internal list used by the Strategy. 
    // We hide the setter to force usage of the nice Builder.
    internal List<AutoStorageRule> AutoRules { get; private set; } = [];

    /// <summary>
    /// Configures the "Smart Tiering" logic for storage.
    /// Define tiers from smallest to largest.
    /// </summary>
    public void ConfigureAutoStrategy(Action<StorageAutoRulesBuilder> configure)
    {
        var builder = new StorageAutoRulesBuilder();
        configure(builder);
        AutoRules = builder.Build();
    }
}
