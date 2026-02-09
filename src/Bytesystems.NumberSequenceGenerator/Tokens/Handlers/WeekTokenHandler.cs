using System.Globalization;

namespace Bytesystems.NumberSequenceGenerator.Tokens.Handlers;

/// <summary>
/// Handles the ISO week number token {w} or {W}. Returns the current ISO 8601 week number, zero-padded to 2 digits.
/// </summary>
public class WeekTokenHandler : ITokenHandler
{
    public bool Handles(Token token) =>
        token.Identifier is "w" or "W";

    public string GetValue(Token token, int sequenceValue)
    {
        if (!Handles(token))
            throw new InvalidOperationException($"Invalid token for WeekTokenHandler: {token.Identifier}");

        var week = ISOWeek.GetWeekOfYear(DateTime.UtcNow);
        return week.ToString("D2");
    }

    public bool RequestsReset(Token token) => false;
}
