using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Domain.Tests.ValueObjects;

public class ShortCodeTests
{
    [Theory]
    [InlineData("exmpl")] // 5
    [InlineData("AbC123")] // 6
    [InlineData("A1b2")]   // 4
    [InlineData("AbCdE12")] // 7 (limite)
    public void Create_ShouldCreateShortCodeInstance_WhenShortCodeIsValid(string value)
    {
        // Act
        var sc = ShortCode.Create(value);

        // Assert
        Assert.NotNull(sc);
        Assert.Equal(value, sc.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowShortCodeNotNullOrEmptyException_WhenValueIsNullOrWhitespace(string? value)
    {
        // Act
        var act = () => ShortCode.Create(value!);

        // Assert
        Assert.Throws<ShortCodeNotNullOrEmptyException>(act);
    }

    [Theory]
    [InlineData("abcdefgh")] // too long (>7)
    [InlineData("ab_cd")] // underscore not allowed
    [InlineData("ab-cd")] // hyphen not allowed
    [InlineData("ab cd")] // space not allowed
    [InlineData("áéíóú")] // non-ascii
    public void Create_ShouldThrowShortCodeInvalidFormatException_WhenShortCodeIsInvalid(string value)
    {
        // Act
        var act = () => ShortCode.Create(value);

        // Assert
        Assert.Throws<ShortCodeInvalidFormatException>(act);
    }

    [Fact]
    public void Equals_ShouldBeTrue_ForSameValue()
    {
        // Arrange
        var a = ShortCode.Create("AbC123");
        var b = ShortCode.Create("AbC123");

        // Assert
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldBeFalse_ForDifferentValues()
    {
        // Arrange
        var a = ShortCode.Create("Code01");
        var b = ShortCode.Create("Code02");

        // Assert
        Assert.False(a == b);
        Assert.NotEqual(a, b);
    }
}
