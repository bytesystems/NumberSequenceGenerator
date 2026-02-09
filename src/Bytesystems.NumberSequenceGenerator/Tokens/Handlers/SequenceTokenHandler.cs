using System.Globalization;

namespace Bytesystems.NumberSequenceGenerator.Tokens.Handlers;

/// <summary>
/// Handles the sequence number token {#} with optional padding and reset context.
/// <list type="bullet">
/// <item>{#} - plain sequence number</item>
/// <item>{#|6} - zero-padded to 6 digits</item>
/// <item>{#|6|y} - zero-padded to 6 digits, resets yearly</item>
/// <item>{#|6|m} - resets monthly, {#|6|d} - resets daily, {#|6|w} - resets weekly, {#|6|h} - resets hourly</item>
/// </list>
/// </summary>
public class SequenceTokenHandler : ITokenHandler
{
    private static readonly string[] AllowedResetContexts = ["y", "m", "w", "d", "h"];

    public bool Handles(Token token) => token.Identifier == "#";

    public string GetValue(Token token, int sequenceValue)
    {
        if (!Handles(token))
            throw new InvalidOperationException($"Invalid token for SequenceTokenHandler: {token.Identifier}");

        var padding = token.Parameters.Count > 0 && int.TryParse(token.Parameters[0], out var p) ? p : 0;
        return sequenceValue.ToString($"D{padding}");
    }

    public bool RequestsReset(Token token)
    {
        if (token.Parameters.Count < 2)
            return false;

        var resetContext = token.Parameters[1].ToLowerInvariant();

        if (!AllowedResetContexts.Contains(resetContext))
            throw new ArgumentException($"Cannot reset on period '{resetContext}'. Allowed: {string.Join(", ", AllowedResetContexts)}");

        var lastUpdate = token.ResetContext;
        var now = DateTime.UtcNow;

        return resetContext switch
        {
            "y" => lastUpdate.Year != now.Year,
            "m" => lastUpdate.Year != now.Year || lastUpdate.Month != now.Month,
            "w" => GetIsoWeek(lastUpdate) != GetIsoWeek(now) || lastUpdate.Year != now.Year,
            "d" => lastUpdate.Date != now.Date,
            "h" => lastUpdate.Date != now.Date || lastUpdate.Hour != now.Hour,
            _ => false
        };
    }

    private static int GetIsoWeek(DateTime date)
    {
        return ISOWeek.GetWeekOfYear(date);
    }
}
