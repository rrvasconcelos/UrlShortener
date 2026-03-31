using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.IdGeneration;
using UrlShortener.Application.UseCases.Common;
using UrlShortener.Application.UseCases.Shorteners.GetByShortCode;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Tests.UseCases.Shorteners.GetByShortCode;

public class GetUrlQueryHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetUrlQueryHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IBase62Encoder _base62Encoder;
    private readonly GetUrlQueryHandler _handler;

    public GetUrlQueryHandlerTests()
    {
        _dbContext = Substitute.For<IApplicationDbContext>();
        _logger = Substitute.For<ILogger<GetUrlQueryHandler>>();
        _cacheService = Substitute.For<ICacheService>();
        _base62Encoder = Substitute.For<IBase62Encoder>();

        _handler = new GetUrlQueryHandler(_dbContext, _logger, _cacheService, _base62Encoder);
    }

    [Fact]
    public async Task Handle_ShouldReturnFromCache_WhenUrlExistsInCache()
    {
        // Arrange
        var shortCode = "abc123";
        var expectedLongUrl = "https://example.com";
        var query = new GetUrlQuery(shortCode);

        _cacheService
            .GetStringAsync($"short:{shortCode}")
            .Returns(expectedLongUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortCode.Should().Be(shortCode);
        result.Value.LongUrl.Should().Be(expectedLongUrl);

        await _cacheService.Received(1).GetStringAsync($"short:{shortCode}");
        _base62Encoder.DidNotReceive().Decode(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenShortCodeIsInvalid()
    {
        // Arrange
        var shortCode = "invalid";
        var query = new GetUrlQuery(shortCode);

        _cacheService
            .GetStringAsync($"short:{shortCode}")
            .Returns((string?)null);

        _base62Encoder
            .Decode(shortCode)
            .Returns(0L); // Invalid ID

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UrlShortCode.NotFound");
        result.Error.Description.Should().Be("URL not found");

        _base62Encoder.Received(1).Decode(shortCode);
    }

    [Fact]
    public void Handle_ShouldThrowException_WhenInvalidShortCodeProvided()
    {
        // Arrange & Act & Assert
        var act = () => ShortCode.Create(""); // Empty short code
        act.Should().Throw<ShortCodeNotNullOrEmptyException>();
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("xyz789")]
    [InlineData("def456")]
    public async Task Handle_ShouldGenerateCorrectCacheKey_WhenCallingCache(string shortCode)
    {
        // Arrange
        var query = new GetUrlQuery(shortCode);
        var expectedCacheKey = $"short:{shortCode}";

        _cacheService
            .GetStringAsync(expectedCacheKey)
            .Returns("https://example.com");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cacheService.Received(1).GetStringAsync(expectedCacheKey);
    }
}