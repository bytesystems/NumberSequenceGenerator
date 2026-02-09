using System.Text.RegularExpressions;
using Bytesystems.NumberSequenceGenerator.Attributes;
using Bytesystems.NumberSequenceGenerator.Entity;
using Bytesystems.NumberSequenceGenerator.Tokens;
using Microsoft.EntityFrameworkCore;

namespace Bytesystems.NumberSequenceGenerator.Services;

/// <summary>
/// Core number generation service. Tokenizes patterns, checks for resets,
/// increments the sequence counter, and produces the formatted number string.
/// </summary>
public partial class NumberGenerator
{
    private readonly TokenHandlerRegistry _tokenHandlerRegistry;

    public NumberGenerator(TokenHandlerRegistry tokenHandlerRegistry)
    {
        _tokenHandlerRegistry = tokenHandlerRegistry;
    }

    /// <summary>
    /// Generates the next number for the given sequence configuration.
    /// Creates the sequence record if it doesn't exist yet.
    /// </summary>
    /// <param name="context">The DbContext to use for sequence persistence.</param>
    /// <param name="attribute">The sequence attribute configuration.</param>
    /// <param name="segmentValue">The resolved segment value (null for non-segmented sequences).</param>
    /// <param name="segmentAttribute">Optional segment-specific pattern override.</param>
    /// <returns>The generated number string.</returns>
    public async Task<string> GetNextNumberAsync(
        DbContext context,
        SequenceAttribute attribute,
        string? segmentValue = null,
        SegmentAttribute? segmentAttribute = null)
    {
        var sequence = await GetOrCreateSequenceAsync(context, attribute, segmentValue, segmentAttribute);

        var pattern = sequence.Pattern;
        var tokens = Tokenize(pattern, sequence.UpdatedAt);

        if (CheckForReset(tokens))
        {
            sequence.CurrentNumber = attribute.Init;
        }

        var nextNumber = sequence.GetNextNumber(attribute.Init);

        return Replace(tokens, pattern, nextNumber);
    }

    /// <summary>
    /// Retrieves the existing sequence record or creates a new one.
    /// For segmented sequences, falls back to the default sequence if no segment-specific record exists.
    /// </summary>
    private async Task<NumberSequence> GetOrCreateSequenceAsync(
        DbContext context,
        SequenceAttribute attribute,
        string? segmentValue,
        SegmentAttribute? segmentAttribute)
    {
        var sequenceSet = context.Set<NumberSequence>();
        var defaultPattern = attribute.Pattern;

        // Try to find or create the default (non-segmented) sequence
        var defaultSequence = await sequenceSet
            .FirstOrDefaultAsync(s => s.Key == attribute.Key && s.Segment == null);

        if (defaultSequence == null)
        {
            defaultSequence = new NumberSequence
            {
                Key = attribute.Key,
                Segment = null,
                Pattern = defaultPattern,
                CurrentNumber = attribute.Init,
            };
            sequenceSet.Add(defaultSequence);
        }

        // If no segment requested, return the default sequence
        if (segmentValue == null)
            return defaultSequence;

        // Try to find an existing segmented sequence
        var segmentedSequence = await sequenceSet
            .FirstOrDefaultAsync(s => s.Key == attribute.Key && s.Segment == segmentValue);

        if (segmentedSequence != null)
            return segmentedSequence;

        // Create a new segmented sequence.
        // If a SegmentAttribute defines a specific pattern, use it; otherwise use the default pattern.
        segmentedSequence = new NumberSequence
        {
            Key = attribute.Key,
            Segment = segmentValue,
            Pattern = segmentAttribute?.Pattern ?? defaultPattern,
            CurrentNumber = attribute.Init,
        };
        sequenceSet.Add(segmentedSequence);
        return segmentedSequence;
    }

    /// <summary>
    /// Parses a pattern string into a list of tokens.
    /// </summary>
    internal List<Token> Tokenize(string pattern, DateTime lastUpdate)
    {
        var tokens = new List<Token>();
        var matches = TokenRegex().Matches(pattern);

        foreach (Match match in matches)
        {
            var info = match.Groups[1].Value.Split('|');
            tokens.Add(new Token(info, match.Value, lastUpdate));
        }

        return tokens;
    }

    /// <summary>
    /// Checks if any token handler requests a sequence reset.
    /// </summary>
    internal bool CheckForReset(List<Token> tokens)
    {
        foreach (var token in tokens)
        {
            foreach (var handler in _tokenHandlerRegistry.Handlers)
            {
                if (handler.Handles(token) && handler.RequestsReset(token))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Replaces all tokens in the pattern with their generated values.
    /// </summary>
    internal string Replace(List<Token> tokens, string pattern, int sequenceValue)
    {
        var result = pattern;
        foreach (var token in tokens)
        {
            foreach (var handler in _tokenHandlerRegistry.Handlers)
            {
                if (handler.Handles(token))
                {
                    result = result.Replace(token.ReplaceToken, handler.GetValue(token, sequenceValue));
                    break;
                }
            }
        }
        return result;
    }

    [GeneratedRegex(@"\{([^|}]*?\|?.*?)\}", RegexOptions.None)]
    private static partial Regex TokenRegex();
}
