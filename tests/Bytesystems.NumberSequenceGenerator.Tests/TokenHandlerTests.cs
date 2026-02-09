using Bytesystems.NumberSequenceGenerator.Tokens;
using Bytesystems.NumberSequenceGenerator.Tokens.Handlers;

namespace Bytesystems.NumberSequenceGenerator.Tests;

public class SequenceTokenHandlerTests
{
    private readonly SequenceTokenHandler _handler = new();

    [Fact]
    public void Handles_SequenceToken_ReturnsTrue()
    {
        var token = new Token(["#"], "{#}", DateTime.UtcNow);
        _handler.Handles(token).Should().BeTrue();
    }

    [Fact]
    public void Handles_NonSequenceToken_ReturnsFalse()
    {
        var token = new Token(["Y"], "{Y}", DateTime.UtcNow);
        _handler.Handles(token).Should().BeFalse();
    }

    [Fact]
    public void GetValue_NoPadding_ReturnsPlainNumber()
    {
        var token = new Token(["#"], "{#}", DateTime.UtcNow);
        _handler.GetValue(token, 42).Should().Be("42");
    }

    [Fact]
    public void GetValue_WithPadding_ReturnsZeroPaddedNumber()
    {
        var token = new Token(["#", "6"], "{#|6}", DateTime.UtcNow);
        _handler.GetValue(token, 42).Should().Be("000042");
    }

    [Fact]
    public void GetValue_WithPaddingAndReset_ReturnsZeroPaddedNumber()
    {
        var token = new Token(["#", "4", "y"], "{#|4|y}", DateTime.UtcNow);
        _handler.GetValue(token, 7).Should().Be("0007");
    }

    [Fact]
    public void RequestsReset_NoResetContext_ReturnsFalse()
    {
        var token = new Token(["#", "6"], "{#|6}", DateTime.UtcNow);
        _handler.RequestsReset(token).Should().BeFalse();
    }

    [Fact]
    public void RequestsReset_YearlyReset_SameYear_ReturnsFalse()
    {
        var token = new Token(["#", "6", "y"], "{#|6|y}", DateTime.UtcNow);
        _handler.RequestsReset(token).Should().BeFalse();
    }

    [Fact]
    public void RequestsReset_YearlyReset_DifferentYear_ReturnsTrue()
    {
        var lastYear = DateTime.UtcNow.AddYears(-1);
        var token = new Token(["#", "6", "y"], "{#|6|y}", lastYear);
        _handler.RequestsReset(token).Should().BeTrue();
    }

    [Fact]
    public void RequestsReset_MonthlyReset_DifferentMonth_ReturnsTrue()
    {
        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        var token = new Token(["#", "6", "m"], "{#|6|m}", lastMonth);
        _handler.RequestsReset(token).Should().BeTrue();
    }

    [Fact]
    public void RequestsReset_DailyReset_DifferentDay_ReturnsTrue()
    {
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var token = new Token(["#", "4", "d"], "{#|4|d}", yesterday);
        _handler.RequestsReset(token).Should().BeTrue();
    }

    [Fact]
    public void RequestsReset_DailyReset_SameDay_ReturnsFalse()
    {
        var token = new Token(["#", "4", "d"], "{#|4|d}", DateTime.UtcNow);
        _handler.RequestsReset(token).Should().BeFalse();
    }

    [Fact]
    public void RequestsReset_InvalidContext_ThrowsArgumentException()
    {
        var token = new Token(["#", "4", "x"], "{#|4|x}", DateTime.UtcNow);
        var act = () => _handler.RequestsReset(token);
        act.Should().Throw<ArgumentException>().WithMessage("*Cannot reset on period 'x'*");
    }
}

public class DateTokenHandlerTests
{
    private readonly DateTokenHandler _handler = new();

    [Theory]
    [InlineData("Y")]
    [InlineData("y")]
    [InlineData("m")]
    [InlineData("M")]
    [InlineData("d")]
    [InlineData("D")]
    [InlineData("H")]
    public void Handles_ValidDateTokens_ReturnsTrue(string identifier)
    {
        var token = new Token([identifier], $"{{{identifier}}}", DateTime.UtcNow);
        _handler.Handles(token).Should().BeTrue();
    }

    [Fact]
    public void Handles_NonDateToken_ReturnsFalse()
    {
        var token = new Token(["#"], "{#}", DateTime.UtcNow);
        _handler.Handles(token).Should().BeFalse();
    }

    [Fact]
    public void GetValue_FourDigitYear_ReturnsCurrentYear()
    {
        var token = new Token(["Y"], "{Y}", DateTime.UtcNow);
        var result = _handler.GetValue(token, 0);
        result.Should().Be(DateTime.UtcNow.ToString("yyyy"));
    }

    [Fact]
    public void GetValue_TwoDigitYear_ReturnsTwoDigitYear()
    {
        var token = new Token(["y"], "{y}", DateTime.UtcNow);
        var result = _handler.GetValue(token, 0);
        result.Should().Be(DateTime.UtcNow.ToString("yy"));
    }

    [Fact]
    public void GetValue_Month_ReturnsTwoDigitMonth()
    {
        var token = new Token(["m"], "{m}", DateTime.UtcNow);
        var result = _handler.GetValue(token, 0);
        result.Should().Be(DateTime.UtcNow.ToString("MM"));
    }

    [Fact]
    public void RequestsReset_AlwaysReturnsFalse()
    {
        var token = new Token(["Y"], "{Y}", DateTime.UtcNow);
        _handler.RequestsReset(token).Should().BeFalse();
    }
}

public class WeekTokenHandlerTests
{
    private readonly WeekTokenHandler _handler = new();

    [Theory]
    [InlineData("w")]
    [InlineData("W")]
    public void Handles_WeekTokens_ReturnsTrue(string identifier)
    {
        var token = new Token([identifier], $"{{{identifier}}}", DateTime.UtcNow);
        _handler.Handles(token).Should().BeTrue();
    }

    [Fact]
    public void GetValue_ReturnsZeroPaddedWeekNumber()
    {
        var token = new Token(["w"], "{w}", DateTime.UtcNow);
        var result = _handler.GetValue(token, 0);
        result.Should().MatchRegex(@"^\d{2}$");
    }

    [Fact]
    public void RequestsReset_AlwaysReturnsFalse()
    {
        var token = new Token(["w"], "{w}", DateTime.UtcNow);
        _handler.RequestsReset(token).Should().BeFalse();
    }
}
