using FlowWire.Framework.Abstractions.Storage;

namespace FlowWire.Framework.Abstractions.Configuration;

public class StorageAutoRulesBuilder
{
    private readonly List<AutoStorageRule> _rules = [];

    /// <summary>
    /// Stores data up to the specified size directly in the State (Redis).
    /// </summary>
    public StorageAutoRulesBuilder UseInlineBelow(long maxBytes)
    {
        _rules.Add(new AutoStorageRule(maxBytes, StorageDecision.Inline()));
        return this;
    }

    /// <summary>
    /// Stores data up to the specified size in the specific Storage Provider.
    /// </summary>
    /// <typeparam name="TStorage">The type of the Storage Provider.</typeparam>
    /// <param name="maxBytes">The maximum size in bytes for this rule.</param>
    public StorageAutoRulesBuilder UseBelow<TStorage>(long maxBytes) where TStorage : IStorage
    {
        _rules.Add(new AutoStorageRule(maxBytes, StorageDecision.StoreIn<TStorage>()));
        return this;
    }

    /// <summary>
    /// Catch-all: Anything larger than the previous rules goes here.
    /// This ends the configuration.
    /// </summary>
    public void DefaultTo<TStorage>() where TStorage : IStorage
    {
        _rules.Add(new AutoStorageRule(long.MaxValue, StorageDecision.StoreIn<TStorage>()));
    }

    /// <summary>
    /// Builds and returns a ordered list of auto storage rules.
    /// </summary>
    internal List<AutoStorageRule> Build()
    {
        // Sort by size to ensure the logic flows correctly (Small -> Large)
        return [.. _rules.OrderBy(x => x.MaxBytes)];
    }
}
