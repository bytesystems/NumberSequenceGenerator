namespace Bytesystems.NumberSequenceGenerator.Tokens.Handlers;

/// <summary>
/// Handles date-related tokens in patterns. Supported tokens map to .NET date format strings:
/// <list type="bullet">
/// <item>{d} - day of month, 2 digits (01-31)</item>
/// <item>{D} - abbreviated day name (Mon, Tue, ...)</item>
/// <item>{m} - month, 2 digits (01-12)</item>
/// <item>{M} - abbreviated month name (Jan, Feb, ...)</item>
/// <item>{y} - year, 2 digits (25)</item>
/// <item>{Y} - year, 4 digits (2025)</item>
/// <item>{H} - hour, 24-hour format, 2 digits (00-23)</item>
/// </list>
/// </summary>
public class DateTokenHandler : ITokenHandler
{
    private static readonly Dictionary<string, string> FormatMap = new()
    {
        ["d"] = "dd",
        ["D"] = "ddd",
        ["m"] = "MM",
        ["M"] = "MMM",
        ["y"] = "yy",
        ["Y"] = "yyyy",
        ["H"] = "HH",
    };

    public bool Handles(Token token) => FormatMap.ContainsKey(token.Identifier);

    public string GetValue(Token token, int sequenceValue)
    {
        if (!Handles(token))
            throw new InvalidOperationException($"Invalid token for DateTokenHandler: {token.Identifier}");

        var format = FormatMap[token.Identifier];
        return DateTime.UtcNow.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
    }

    public bool RequestsReset(Token token) => false;
}
