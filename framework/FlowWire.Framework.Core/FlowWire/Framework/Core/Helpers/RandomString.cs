namespace FlowWire.Framework.Core.Helpers;

public static class RandomString
{
    // A-Z
    public const string AlphaUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    // a-z
    public const string AlphaLower = "abcdefghijklmnopqrstuvwxyz";

    // A-Z, a-z
    public const string Alpha = AlphaUpper + AlphaLower;

    // 0-9
    public const string Numeric = "0123456789";

    // A-Z, a-z, 0-9
    public const string Alphanumeric = Alpha + Numeric;

    // 0-9, A-F (Upper)
    public const string Hex = Numeric + "ABCDEF";

    // Standard special symbols
    public const string Symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    // Alphanumeric + Symbols
    public const string AlphanumericSymbols = Alphanumeric + Symbols;


    /// <summary>
    /// Generates a random string using the specified character strategy.
    /// Uses string.Create and Random.Shared.GetItems for zero-allocation performance.
    /// </summary>
    /// <param name="length">The length of the string to generate.</param>
    /// <param name="strategy">The set of characters to choose from.</param>
    public static string Generate(int length, string strategy = Alphanumeric)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentException.ThrowIfNullOrEmpty(strategy);

        return string.Create(length, strategy, (span, chars) =>
        {
            Random.Shared.GetItems(chars, span);
        });
    }
}