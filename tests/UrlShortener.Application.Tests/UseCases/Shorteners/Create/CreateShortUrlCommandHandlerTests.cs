using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.IdGeneration;
using UrlShortener.Application.UseCases.Shorteners.Create;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.Tests.UseCases.Shorteners.Create;

public class CreateShortUrlCommandHandlerTests
{
    private readonly ISnowflakeIdGenerator _idGenerator;
    private readonly IBase62Encoder _base62Encoder;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateShortUrlCommandHandler> _logger;
    private readonly CreateShortUrlCommandHandler _handler;

    public CreateShortUrlCommandHandlerTests()
    {
        _idGenerator = Substitute.For<ISnowflakeIdGenerator>();
        _base62Encoder = Substitute.For<IBase62Encoder>();
        _dbContext = Substitute.For<IApplicationDbContext>();
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<CreateShortUrlCommandHandler>>();
        _handler = new CreateShortUrlCommandHandler(_idGenerator, _base62Encoder, _dbContext, _cacheService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnFromCache_WhenUrlExistsInCache()
    {
        // Arrange
        var command = new CreateShortUrlCommand { LongUrl = new Uri("https://example.com") };
        var expectedShortCode = "abc123";

        _cacheService.GetStringAsync("long:https://example.com/")
            .Returns(expectedShortCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortCode.Should().Be(expectedShortCode);

        await _cacheService.Received(1).GetStringAsync("long:https://example.com/");
    }

    [Fact]
    public void Handle_ShouldThrowException_WhenInvalidUrlProvided()
    {
        // Arrange & Act & Assert
        var act = () => LongUrl.Create("http://invalid url");
        act.Should().Throw<LongUrlInvalidFormatException>();
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("https://www.google.com/search?q=test")]
    [InlineData("https://github.com/user/repo")]
    public async Task Handle_ShouldCreateValidLongUrl_WhenValidUrlProvided(string url)
    {
        // Arrange
        var command = new CreateShortUrlCommand { LongUrl = new Uri(url) };

        _cacheService.GetStringAsync(Arg.Any<string>())
            .Returns("cached123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ShortCode.Should().Be("cached123");
    }

    [Fact]
    public async Task Handle_ShouldGenerateCorrectCacheKey_WhenCallingCache()
    {
        // Arrange
        var command = new CreateShortUrlCommand { LongUrl = new Uri("https://example.com/path") };
        var expectedCacheKey = "long:https://example.com/path";

        _cacheService.GetStringAsync(expectedCacheKey)
            .Returns("cached123");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _cacheService.Received(1).GetStringAsync(expectedCacheKey);
    }
}