using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Analytics;

internal sealed class GetUrlAnalyticsQueryHandler(
    IApplicationDbContext dbContext,
    ILogger<GetUrlAnalyticsQueryHandler> logger)
    : IQueryHandler<GetUrlAnalyticsQuery, UrlAnalyticsResponse>
{
    public async Task<Result<UrlAnalyticsResponse>> Handle(GetUrlAnalyticsQuery query, CancellationToken cancellationToken)
    {
        var exists = await dbContext.UrlMappings
            .AnyAsync(m => m.ShortCode != null && m.ShortCode.Value == query.ShortCode, cancellationToken);

        if (!exists)
        {
            logger.LogWarning("Analytics requested for non-existent short code: {ShortCode}", query.ShortCode);
            return Result.Failure<UrlAnalyticsResponse>(Error.NotFound("Analytics.NotFound", "Short URL not found"));
        }

        var clicks = await dbContext.UrlClicks
            .Where(c => c.ShortCode == query.ShortCode)
            .ToListAsync(cancellationToken);

        var totalClicks = clicks.LongCount();

        var clicksByDay = clicks
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new ClicksByDay(g.Key, g.Count()))
            .OrderByDescending(c => c.Date)
            .ToList();

        return Result.Success(new UrlAnalyticsResponse
        {
            ShortCode = query.ShortCode,
            TotalClicks = totalClicks,
            ClicksByDay = clicksByDay
        });
    }
}
