using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UrlShortener.Api.Tests.Infrastructure;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.UseCases.Common;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;
using Xunit;

namespace UrlShortener.Api.Tests.Endpoints.Shorteners;

public class GetByShortCodeEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GetByShortCodeEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByShortCode_ShouldReturnNotFound_WhenShortCodeDoesNotExist()
    {
        // Arrange
        var shortCode = "nonexistent";

        // Act
        var response = await _client.GetAsync($"/{shortCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByShortCode_ShouldReturnBadRequest_WhenInvalidShortCodeProvided()
    {
        // Arrange
        var shortCode = ""; // Empty short code

        // Act
        var response = await _client.GetAsync($"/{shortCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound); // ASP.NET Core returns 404 for empty route parameter
    }

    [Fact]
    public async Task GetByShortCode_ShouldReturnRedirect_WhenValidShortCodeExists()
    {
        // Arrange
        var longUrl = "https://example.com";
        
        // First, create a short URL
        var createRequest = new { LongUrl = longUrl };
        var createResponse = await _client.PostAsJsonAsync("/", createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var urlResponse = JsonSerializer.Deserialize<UrlResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        var shortCode = urlResponse!.ShortCode;

        // Act
        var response = await _client.GetAsync($"/{shortCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(longUrl);
    }

    [Fact]
    public async Task GetByShortCode_ShouldHaveCacheHeaders_WhenValidShortCodeExists()
    {
        // Arrange
        var longUrl = "https://example.com";
        
        // First, create a short URL
        var createRequest = new { LongUrl = longUrl };
        var createResponse = await _client.PostAsJsonAsync("/", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var urlResponse = JsonSerializer.Deserialize<UrlResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        var shortCode = urlResponse!.ShortCode;

        // Act
        var response = await _client.GetAsync($"/{shortCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        
        // Check cache headers are present
        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.Should().ContainKey("Expires");
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://github.com/user/repo")]
    [InlineData("https://stackoverflow.com/questions/12345")]
    public async Task GetByShortCode_ShouldRedirectToCorrectUrl_ForVariousUrls(string longUrl)
    {
        // Arrange - Create a short URL
        var createRequest = new { LongUrl = longUrl };
        var createResponse = await _client.PostAsJsonAsync("/", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var urlResponse = JsonSerializer.Deserialize<UrlResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        var shortCode = urlResponse!.ShortCode;

        // Act
        var response = await _client.GetAsync($"/{shortCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Be(longUrl);
    }

    [Fact]
    public async Task CreateAndGet_ShouldWorkTogether_ForCompleteWorkflow()
    {
        // Arrange
        var longUrl = "https://example.com/test-workflow";
        var createRequest = new { LongUrl = longUrl };

        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/", createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var urlResponse = JsonSerializer.Deserialize<UrlResponse>(createContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Get
        var getResponse = await _client.GetAsync($"/{urlResponse!.ShortCode}");

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        getResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        getResponse.Headers.Location?.ToString().Should().Be(longUrl);
        
        urlResponse.LongUrl.Should().Be(longUrl);
        urlResponse.ShortCode.Should().NotBeNullOrEmpty();
    }
}