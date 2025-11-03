using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Domain.Tests.ValueObjects;

public class LongUrlTests
{
    [Fact]
    public void Create_ShouldCreateLongUrlInstance_WhenUrlIsValid()
    {
        // Arrange
        var value = "https://www.example.com/some/long/path?x=1#frag";

        // Act
        var longUrl = LongUrl.Create(value);

        // Assert
        Assert.NotNull(longUrl);
        Assert.Equal(value, longUrl.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowLongUrlNotNullOrEmptyException_WhenValueIsNullOrWhitespace(string? value)
    {
        // Act
        var act = () => LongUrl.Create(value!);

        // Assert
        Assert.Throws<LongUrlNotNullOrEmptyException>(act);
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("http:/bad.com")] // malformed
    [InlineData("www.example.com")] // missing scheme (not absolute)
    [InlineData("/relative/path")] // relative path
    public void Create_ShouldThrowLongUrlInvalidFormatException_WhenUrlIsInvalid(string value)
    {
        // Act
        var act = () => LongUrl.Create(value);

        // Assert
        Assert.Throws<LongUrlInvalidFormatException>(act);
    }

    [Fact]
    public void Equals_ShouldBeTrue_ForSameValue()
    {
        // Arrange
        var url = "https://example.com";

        // Act
        var a = LongUrl.Create(url);
        var b = LongUrl.Create(url);

        // Assert
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldBeFalse_ForDifferentValues()
    {
        // Arrange
        var a = LongUrl.Create("https://example.com/a");
        var b = LongUrl.Create("https://example.com/b");

        // Assert
        Assert.False(a == b);
        Assert.NotEqual(a, b);
    }
}
