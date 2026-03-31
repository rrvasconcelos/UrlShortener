using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.IdGeneration;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Common;
using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners.GetByShortCode;

public class GetUrlQueryHandler(
    IApplicationDbContext context,
    ILogger<GetUrlQueryHandler> logger,
    ICacheService cacheService,
    IBase62Encoder base62Encoder)
    : IQueryHandler<GetUrlQuery, UrlResponse>
{
    private const int CacheExpirationDays = 30;

    public async Task<Result<UrlResponse>> Handle(GetUrlQuery query, CancellationToken cancellationToken)
    {
        var shortCode = ShortCode.Create(query.ShortCode);

        var cacheResult = await TryGetFromCacheAsync(shortCode);
        if (cacheResult is not null)
            return cacheResult;

        return await GetFromDatabaseAsync(shortCode, cancellationToken);
    }

    private async Task<Result<UrlResponse>?> TryGetFromCacheAsync(ShortCode shortCode)
    {
        try
        {
            var cacheKey = GenerateCacheKey(shortCode);
            var cachedLongUrl = await cacheService.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(cachedLongUrl))
                return null;

            logger.LogInformation("Long URL for short code: {ShortCode} found in cache", shortCode.Value);

            return Result.Success(new UrlResponse
            {
                ShortCode = shortCode.Value,
                LongUrl = cachedLongUrl
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve from cache for short code: {ShortCode}", shortCode.Value);
            return null;
        }
    }

    private async Task<Result<UrlResponse>> GetFromDatabaseAsync(ShortCode shortCode, CancellationToken cancellationToken)
    {
        long urlMappingId;
        try
        {
            urlMappingId = base62Encoder.Decode(shortCode.Value);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decode short code: {ShortCode}", shortCode.Value);
            return Result.Failure<UrlResponse>(Error.NotFound("UrlShortCode.NotFound", "URL not found"));
        }

        if (urlMappingId <= 0)
        {
            logger.LogWarning("Invalid short code format: {ShortCode}", shortCode.Value);
            return Result.Failure<UrlResponse>(Error.NotFound("UrlShortCode.NotFound", "URL not found"));
        }

        var urlMapping = await context.UrlMappings
            .FirstOrDefaultAsync(e => e.Id == urlMappingId, cancellationToken);

        if (urlMapping?.LongUrl?.Value is not { } longUrl)
        {
            logger.LogInformation("URL mapping not found for short code: {ShortCode}", shortCode.Value);
            return Result.Failure<UrlResponse>(Error.NotFound("UrlShortCode.NotFound", "URL not found"));
        }

        logger.LogInformation("Long URL for short code: {ShortCode} found in database", shortCode.Value);

        _ = Task.Run(async () => await CacheUrlMappingAsync(shortCode, longUrl), cancellationToken);

        return Result.Success(new UrlResponse
        {
            ShortCode = shortCode.Value,
            LongUrl = longUrl
        });
    }

    private async Task CacheUrlMappingAsync(ShortCode shortCode, string longUrl)
    {
        try
        {
            var cacheKey = GenerateCacheKey(shortCode);
            await cacheService.SetStringAsync(cacheKey, longUrl, TimeSpan.FromDays(CacheExpirationDays));
            logger.LogDebug("Cached URL mapping for short code: {ShortCode}", shortCode.Value);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cache URL mapping for short code: {ShortCode}", shortCode.Value);
        }
    }

    private static string GenerateCacheKey(ShortCode shortCode) => $"short:{shortCode.Value}";
}