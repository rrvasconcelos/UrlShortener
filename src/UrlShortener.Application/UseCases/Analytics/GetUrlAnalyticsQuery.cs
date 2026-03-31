using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.UseCases.Analytics;

public record GetUrlAnalyticsQuery(string ShortCode) : IQuery<UrlAnalyticsResponse>;

public record UrlAnalyticsResponse
{
    public required string ShortCode { get; init; }
    public required long TotalClicks { get; init; }
    public required IReadOnlyList<ClicksByDay> ClicksByDay { get; init; }
}

public record ClicksByDay(DateTime Date, int Count);
