using System.Text.RegularExpressions;
using Bytesystems.NumberSequenceGenerator.Attributes;

namespace Bytesystems.NumberSequenceGenerator.Services;

/// <summary>
/// Resolves segment values from entity properties and matches them to segment-specific pattern definitions.
/// </summary>
public partial class SegmentResolver
{
    private readonly PropertyHelper _propertyHelper;

    public SegmentResolver(PropertyHelper propertyHelper)
    {
        _propertyHelper = propertyHelper;
    }

    /// <summary>
    /// Resolves the segment value by replacing {PropertyName} placeholders in the segment expression
    /// with actual property values from the entity.
    /// </summary>
    /// <returns>The resolved segment string, or null if no segment is defined.</returns>
    public string? ResolveSegmentValue(object entity, SequenceAttribute attribute)
    {
        if (string.IsNullOrEmpty(attribute.Segment))
            return null;

        var segment = attribute.Segment;
        var matches = PropertyPlaceholderRegex().Matches(segment);

        foreach (Match match in matches)
        {
            var propertyName = match.Groups[1].Value;
            var propertyValue = _propertyHelper.GetValue(entity, propertyName);
            segment = segment.Replace(match.Value, propertyValue?.ToString() ?? string.Empty);
        }

        return segment;
    }

    /// <summary>
    /// Finds a matching <see cref="SegmentAttribute"/> for the given segment value
    /// from the types listed in <see cref="SequenceAttribute.Segments"/>.
    /// </summary>
    /// <returns>The matching SegmentAttribute, or null if no match found.</returns>
    public SegmentAttribute? ResolveSegment(SequenceAttribute attribute, string? segmentValue)
    {
        if (attribute.Segments == null || segmentValue == null)
            return null;

        foreach (var segmentType in attribute.Segments)
        {
            var segmentAttr = Attribute.GetCustomAttribute(segmentType, typeof(SegmentAttribute)) as SegmentAttribute;
            if (segmentAttr != null && segmentAttr.Value == segmentValue)
                return segmentAttr;
        }

        return null;
    }

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex PropertyPlaceholderRegex();
}
