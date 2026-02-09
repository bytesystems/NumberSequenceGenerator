using Bytesystems.NumberSequenceGenerator.Tokens;
using Bytesystems.NumberSequenceGenerator.Tokens.Handlers;
using Bytesystems.NumberSequenceGenerator.Services;

namespace Bytesystems.NumberSequenceGenerator.Tests;

public class NumberGeneratorTests
{
    private readonly NumberGenerator _generator;

    public NumberGeneratorTests()
    {
        var handlers = new ITokenHandler[]
        {
            new SequenceTokenHandler(),
            new DateTokenHandler(),
            new WeekTokenHandler()
        };
        var registry = new TokenHandlerRegistry(handlers);
        _generator = new NumberGenerator(registry);
    }

    [Fact]
    public void Tokenize_SimpleSequence_ReturnsSingleToken()
    {
        var tokens = _generator.Tokenize("{#}", DateTime.UtcNow);
        tokens.Should().HaveCount(1);
        tokens[0].Identifier.Should().Be("#");
    }

    [Fact]
    public void Tokenize_PaddedSequence_ReturnsTokenWithParameters()
    {
        var tokens = _generator.Tokenize("{#|6}", DateTime.UtcNow);
        tokens.Should().HaveCount(1);
        tokens[0].Identifier.Should().Be("#");
        tokens[0].Parameters.Should().HaveCount(1);
        tokens[0].Parameters[0].Should().Be("6");
    }

    [Fact]
    public void Tokenize_SequenceWithReset_ReturnsTokenWithTwoParameters()
    {
        var tokens = _generator.Tokenize("{#|6|y}", DateTime.UtcNow);
        tokens.Should().HaveCount(1);
        tokens[0].Parameters.Should().HaveCount(2);
        tokens[0].Parameters[0].Should().Be("6");
        tokens[0].Parameters[1].Should().Be("y");
    }

    [Fact]
    public void Tokenize_ComplexPattern_ReturnsMultipleTokens()
    {
        var tokens = _generator.Tokenize("IV{Y}-{#|6|y}", DateTime.UtcNow);
        tokens.Should().HaveCount(2);
        tokens[0].Identifier.Should().Be("Y");
        tokens[1].Identifier.Should().Be("#");
    }

    [Fact]
    public void Tokenize_PatternWithMonthAndDay_ReturnsCorrectTokens()
    {
        var tokens = _generator.Tokenize("DOC-{y}{m}{d}-{#|5}", DateTime.UtcNow);
        tokens.Should().HaveCount(4);
        tokens[0].Identifier.Should().Be("y");
        tokens[1].Identifier.Should().Be("m");
        tokens[2].Identifier.Should().Be("d");
        tokens[3].Identifier.Should().Be("#");
    }

    [Fact]
    public void Replace_SimplePattern_ReturnsFormattedNumber()
    {
        var tokens = _generator.Tokenize("{#}", DateTime.UtcNow);
        var result = _generator.Replace(tokens, "{#}", 42);
        result.Should().Be("42");
    }

    [Fact]
    public void Replace_PaddedPattern_ReturnsZeroPaddedNumber()
    {
        var tokens = _generator.Tokenize("{#|7}", DateTime.UtcNow);
        var result = _generator.Replace(tokens, "{#|7}", 123);
        result.Should().Be("0000123");
    }

    [Fact]
    public void Replace_ComplexPattern_ReturnsFullFormattedString()
    {
        var now = DateTime.UtcNow;
        var tokens = _generator.Tokenize("IV{Y}-{#|6|y}", now);
        var result = _generator.Replace(tokens, "IV{Y}-{#|6|y}", 4121);
        result.Should().Be($"IV{now:yyyy}-004121");
    }

    [Fact]
    public void Replace_PrefixWithDate_FormatsCorrectly()
    {
        var now = DateTime.UtcNow;
        var pattern = "KD-{y}{m}-{#|4}";
        var tokens = _generator.Tokenize(pattern, now);
        var result = _generator.Replace(tokens, pattern, 23);
        result.Should().Be($"KD-{now:yy}{now:MM}-0023");
    }

    [Fact]
    public void CheckForReset_NoResetTokens_ReturnsFalse()
    {
        var tokens = _generator.Tokenize("{#|6}", DateTime.UtcNow);
        _generator.CheckForReset(tokens).Should().BeFalse();
    }

    [Fact]
    public void CheckForReset_YearlyResetSameYear_ReturnsFalse()
    {
        var tokens = _generator.Tokenize("{#|6|y}", DateTime.UtcNow);
        _generator.CheckForReset(tokens).Should().BeFalse();
    }

    [Fact]
    public void CheckForReset_YearlyResetDifferentYear_ReturnsTrue()
    {
        var lastYear = DateTime.UtcNow.AddYears(-1);
        var tokens = _generator.Tokenize("{#|6|y}", lastYear);
        _generator.CheckForReset(tokens).Should().BeTrue();
    }
}
