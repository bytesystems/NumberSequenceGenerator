using Bytesystems.NumberSequenceGenerator.Attributes;
using Bytesystems.NumberSequenceGenerator.Services;

namespace Bytesystems.NumberSequenceGenerator.Tests;

// Test entities
public class TestOrder
{
    public string OrderType { get; set; } = "OFFER";
    public string? OrderNumber { get; set; }
}

// Segment definitions
[Segment(Value = "OFFER", Pattern = "AG-{#|4}")]
public class OfferSegmentDef;

[Segment(Value = "ORDER", Pattern = "KV-{#|4}")]
public class OrderSegmentDef;

public class SegmentResolverTests
{
    private readonly SegmentResolver _resolver;

    public SegmentResolverTests()
    {
        _resolver = new SegmentResolver(new PropertyHelper());
    }

    [Fact]
    public void ResolveSegmentValue_NoSegment_ReturnsNull()
    {
        var attr = new SequenceAttribute { Key = "test", Pattern = "{#}" };
        var entity = new TestOrder();

        _resolver.ResolveSegmentValue(entity, attr).Should().BeNull();
    }

    [Fact]
    public void ResolveSegmentValue_StaticSegment_ReturnsStaticValue()
    {
        var attr = new SequenceAttribute { Key = "test", Segment = "FIXED" };
        var entity = new TestOrder();

        _resolver.ResolveSegmentValue(entity, attr).Should().Be("FIXED");
    }

    [Fact]
    public void ResolveSegmentValue_PropertyReference_ResolvesFromEntity()
    {
        var attr = new SequenceAttribute { Key = "test", Segment = "{OrderType}" };
        var entity = new TestOrder { OrderType = "OFFER" };

        _resolver.ResolveSegmentValue(entity, attr).Should().Be("OFFER");
    }

    [Fact]
    public void ResolveSegment_MatchingSegment_ReturnsSegmentAttribute()
    {
        var attr = new SequenceAttribute
        {
            Key = "order",
            Segments = [typeof(OfferSegmentDef), typeof(OrderSegmentDef)]
        };

        var result = _resolver.ResolveSegment(attr, "OFFER");

        result.Should().NotBeNull();
        result!.Value.Should().Be("OFFER");
        result.Pattern.Should().Be("AG-{#|4}");
    }

    [Fact]
    public void ResolveSegment_NonMatchingSegment_ReturnsNull()
    {
        var attr = new SequenceAttribute
        {
            Key = "order",
            Segments = [typeof(OfferSegmentDef)]
        };

        _resolver.ResolveSegment(attr, "INVOICE").Should().BeNull();
    }

    [Fact]
    public void ResolveSegment_NullSegments_ReturnsNull()
    {
        var attr = new SequenceAttribute { Key = "test" };

        _resolver.ResolveSegment(attr, "anything").Should().BeNull();
    }
}
