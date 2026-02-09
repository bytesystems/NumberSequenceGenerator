namespace Bytesystems.NumberSequenceGenerator.Attributes;

/// <summary>
/// Defines a segment-specific pattern override for a sequence.
/// Apply this attribute to a class to define a named segment with its own pattern.
/// Reference the class type in <see cref="SequenceAttribute.Segments"/>.
/// </summary>
/// <example>
/// <code>
/// [Segment(Value = "OFFER", Pattern = "O-{#|7}")]
/// public class OfferSegment;
///
/// [Segment(Value = "DELIVERYNOTE", Pattern = "DN-{#|7}")]
/// public class DeliveryNoteSegment;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SegmentAttribute : Attribute
{
    /// <summary>
    /// The segment value that this pattern applies to.
    /// Matched against the resolved segment value from the entity.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// The pattern to use for this segment. Overrides the default pattern from <see cref="SequenceAttribute"/>.
    /// </summary>
    public required string Pattern { get; init; }
}
