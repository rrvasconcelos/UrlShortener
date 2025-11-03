using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UrlShortener.Api.Tests.Infrastructure;
using UrlShortener.Application.UseCases.Common;
using Xunit;

namespace UrlShortener.Api.Tests.Endpoints.Shorteners;

public class CreateShortUrlEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CreateShortUrlEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateShortUrl_ShouldReturnOk_WhenValidUrlProvided()
    {
        // Arrange
        var request = new { LongUrl = "https://example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
        
        var urlResponse = JsonSerializer.Deserialize<UrlResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        urlResponse.Should().NotBeNull();
        urlResponse!.LongUrl.Should().Be("https://example.com");
        urlResponse.ShortCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateShortUrl_ShouldReturnBadRequest_WhenInvalidUrlProvided()
    {
        // Arrange
        var request = new { LongUrl = "invalid-url" };

        // Act
        var response = await _client.PostAsJsonAsync("/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortUrl_ShouldReturnBadRequest_WhenEmptyUrlProvided()
    {
        // Arrange
        var request = new { LongUrl = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateShortUrl_ShouldReturnBadRequest_WhenNullUrlProvided()
    {
        // Arrange
        var request = new { LongUrl = (string?)null };

        // Act
        var response = await _client.PostAsJsonAsync("/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://github.com/user/repo")]
    [InlineData("https://stackoverflow.com/questions/12345")]
    public async Task CreateShortUrl_ShouldReturnValidResponse_ForVariousUrls(string longUrl)
    {
        // Arrange
        var request = new { LongUrl = longUrl };

        // Act
        var response = await _client.PostAsJsonAsync("/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var urlResponse = JsonSerializer.Deserialize<UrlResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        urlResponse.Should().NotBeNull();
        urlResponse!.LongUrl.Should().Be(longUrl);
        urlResponse.ShortCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateShortUrl_ShouldGenerateDifferentShortCodes_ForDifferentUrls()
    {
        // Arrange
        var request1 = new { LongUrl = "https://example1.com" };
        var request2 = new { LongUrl = "https://example2.com" };

        // Act
        var response1 = await _client.PostAsJsonAsync("/", request1);
        var response2 = await _client.PostAsJsonAsync("/", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();

        var urlResponse1 = JsonSerializer.Deserialize<UrlResponse>(content1, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        var urlResponse2 = JsonSerializer.Deserialize<UrlResponse>(content2, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        urlResponse1!.ShortCode.Should().NotBe(urlResponse2!.ShortCode);
    }
}