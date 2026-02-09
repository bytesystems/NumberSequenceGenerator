namespace Bytesystems.NumberSequenceGenerator.Tokens;

/// <summary>
/// Interface for token handlers that process specific tokens in a number pattern.
/// Implementations handle token replacement and optional reset detection.
/// </summary>
public interface ITokenHandler
{
    /// <summary>
    /// Determines whether this handler can process the given token.
    /// </summary>
    bool Handles(Token token);

    /// <summary>
    /// Returns the string value to replace the token with.
    /// </summary>
    /// <param name="token">The token to process.</param>
    /// <param name="sequenceValue">The current sequence number value.</param>
    string GetValue(Token token, int sequenceValue);

    /// <summary>
    /// Determines whether the sequence counter should be reset based on this token's context.
    /// Only the sequence token handler ({#}) typically implements reset logic.
    /// </summary>
    bool RequestsReset(Token token);
}
