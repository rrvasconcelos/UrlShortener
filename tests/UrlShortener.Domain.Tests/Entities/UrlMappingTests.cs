using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Domain.Tests.Entities;

public class UrlMappingTests
{
    [Fact]
    public void Create_ShouldCreateUrlMappingInstance_WhenUrlMappingIsValid()
    {
        // Arrange
        var longUrl = LongUrl.Create("https://www.example.com/some/long/url");
        var shortCode = ShortCode.Create("exmpl");
        DateTime? expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var urlMapping = UrlMapping.Create(longUrl, shortCode, expiresAt);

        // Assert
        Assert.NotNull(urlMapping);
        Assert.Equal(longUrl, urlMapping.LongUrl);
        Assert.Equal(shortCode, urlMapping.ShortCode);
        Assert.Equal(expiresAt, urlMapping.ExpiresAt);
        Assert.Equal(0, urlMapping.ClickCount);
        Assert.True((DateTime.UtcNow - urlMapping.CreatedAt).TotalSeconds < 5);
    }
}

