namespace UrlShortener.Application.UseCases.Shorteners;

public record UrlResponse
{
    public required string ShortCode { get; set; }
}