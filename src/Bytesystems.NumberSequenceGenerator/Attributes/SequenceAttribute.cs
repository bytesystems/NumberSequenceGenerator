namespace Bytesystems.NumberSequenceGenerator.Attributes;

/// <summary>
/// Marks a property to be automatically populated with a generated sequence number on entity creation.
/// The sequence is persisted in the database and supports configurable patterns with date tokens,
/// padding, auto-reset, and segmentation.
/// </summary>
/// <example>
/// <code>
/// [Sequence(Key = "invoice", Pattern = "IV{Y}-{#|6|y}")]
/// public string? InvoiceNumber { get; set; }
///
/// [Sequence(Key = "order", Segment = "{Type}", Pattern = "PO-{#|7}",
///     Segments = new[] { typeof(OfferSegment) })]
/// public string? OrderNumber { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SequenceAttribute : Attribute
{
    /// <summary>
    /// The unique key identifying this sequence. Sequences are stored per key (and optional segment).
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Optional segment expression. Can be a static string or a property reference like "{PropertyName}".
    /// Segments allow different counters/patterns for the same key.
    /// </summary>
    public string? Segment { get; init; }

    /// <summary>
    /// The pattern used to generate the number. Defaults to "{#}" (plain sequence number).
    /// Supports tokens: {#|padding|resetContext}, {Y}, {y}, {m}, {M}, {d}, {D}, {H}, {w}.
    /// </summary>
    public string Pattern { get; init; } = "{#}";

    /// <summary>
    /// Initial value for the sequence counter. The first generated number will be init + 1.
    /// Defaults to 0.
    /// </summary>
    public int Init { get; init; } = 0;

    /// <summary>
    /// Optional array of <see cref="SegmentAttribute"/> types that define segment-specific patterns.
    /// Each type must be a class decorated with <see cref="SegmentAttribute"/>.
    /// </summary>
    public Type[]? Segments { get; init; }
}
