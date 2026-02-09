namespace Bytesystems.NumberSequenceGenerator.Tokens;

/// <summary>
/// Represents a parsed token from a pattern string.
/// A token has an identifier (e.g. "#", "Y", "m") and optional parameters.
/// </summary>
public class Token
{
    /// <summary>
    /// The token identifier (e.g. "#" for sequence, "Y" for year, "m" for month).
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Additional parameters for the token (e.g. padding length, reset context).
    /// </summary>
    public IReadOnlyList<string> Parameters { get; }

    /// <summary>
    /// The original token string including braces, used for replacement (e.g. "{#|6|y}").
    /// </summary>
    public string ReplaceToken { get; }

    /// <summary>
    /// The timestamp of the last sequence update, used for reset context comparison.
    /// </summary>
    public DateTime ResetContext { get; }

    public Token(string[] tokenInfo, string replaceToken, DateTime resetContext)
    {
        Identifier = tokenInfo.Length > 0 ? tokenInfo[0] : string.Empty;
        Parameters = tokenInfo.Length > 1 ? tokenInfo[1..] : [];
        ReplaceToken = replaceToken;
        ResetContext = resetContext;
    }
}
