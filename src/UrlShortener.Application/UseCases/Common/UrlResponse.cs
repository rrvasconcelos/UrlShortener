namespace UrlShortener.Application.UseCases.Shorteners.Create;

public record UrlResponse
{
    public required string ShortCode { get; set; }
}