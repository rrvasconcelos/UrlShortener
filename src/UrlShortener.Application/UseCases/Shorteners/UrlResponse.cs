namespace UrlShortener.Application.UseCases.Shorteners;

public record UrlResponse
{
    public required string ShortCode { get; set; }
    public required string LongUrl { get; set; }
    public long ClickCount { get; set; }
}