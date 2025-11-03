namespace UrlShortener.Application.UseCases.Common;

public record UrlResponse
{
    public required string ShortCode { get; set; }
    public string? LongUrl { get; set; }
}