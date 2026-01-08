using FlowWire.Framework.Abstractions.Configuration;
using Microsoft.Extensions.Options;

namespace FlowWire.Framework.Abstractions.Storage.Strategies;

public sealed class AutoStrategy(IOptions<FlowWireOptions> options) : StorageStrategy
{
    private readonly StorageOptions _options = options.Value.Storage;

    public override StorageDecision Decide(object? value, string propertyName)
    {
        // No rules, Fallback
        if (_options.AutoRules.Count == 0)
        {
            return StorageDecision.Inline();
        }

        var size = EstimateSize(value);

        foreach (var rule in _options.AutoRules)
        {
            if (size <= rule.MaxBytes)
            {
                return rule.Decision;
            }
        }

        // Fallback if no "DefaultTo" (Catch-all) is configured and the object is huge,
        return _options.AutoRules.Last().Decision;
    }

    private static long EstimateSize(object? value)
    {
        if (value is null)
        {
            return 0;
        }

        if (value is byte[] b)
        {
            return b.Length;
        }

        if (value is string s)
        {
            return s.Length * 2; // Rough char estimate
        }
        
        // For complex objects, we default
        return 0;
    }
}