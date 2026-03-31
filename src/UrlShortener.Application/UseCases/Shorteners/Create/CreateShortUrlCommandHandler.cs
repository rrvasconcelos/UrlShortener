using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.IdGeneration;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Common;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Errors;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners.Create;

public class CreateShortUrlCommandHandler(
    ISnowflakeIdGenerator idGenerator,
    IBase62Encoder base62Encoder,
    IApplicationDbContext dbContext,
    ICacheService cacheService,
    ILogger<CreateShortUrlCommandHandler> logger)
    : ICommandHandler<CreateShortUrlCommand, UrlResponse>
{
    private const int CacheExpirationDays = 30;

    public async Task<Result<UrlResponse>> Handle(CreateShortUrlCommand command, CancellationToken cancellationToken)
    {
        var longUrl = LongUrl.Create(command.LongUrl.ToString());

        var cacheResult = await TryGetFromCacheAsync(longUrl);
        if (cacheResult is not null)
            return cacheResult;

        var dbResult = await TryGetFromDatabaseAsync(longUrl, cancellationToken);
        if (dbResult is not null)
            return dbResult;

        return await CreateNewShortUrlAsync(longUrl, cancellationToken);
    }

    private async Task<Result<UrlResponse>?> TryGetFromCacheAsync(LongUrl longUrl)
    {
        try
        {
            var cacheKey = GenerateCacheKey(longUrl);
            var existingShortCode = await cacheService.GetStringAsync(cacheKey);

            if (existingShortCode is null)
                return null;

            logger.LogInformation("Short code for URL: {LongUrl} found in cache", longUrl.Value);
            return Result.Success(new UrlResponse { ShortCode = existingShortCode });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to retrieve from cache for URL: {LongUrl}", longUrl.Value);
            return null;
        }
    }

    private async Task<Result<UrlResponse>?> TryGetFromDatabaseAsync(LongUrl longUrl, CancellationToken cancellationToken)
    {
        var existingMapping = await dbContext.UrlMappings
            .FirstOrDefaultAsync(mapping => mapping.LongUrl.Equals(longUrl), cancellationToken);

        if (existingMapping?.ShortCode?.Value is not { } shortCode)
            return null;

        logger.LogInformation("Short code for URL: {LongUrl} found in database", longUrl.Value);

        _ = Task.Run(async () => await CacheShortCodeAsync(longUrl, shortCode), cancellationToken);

        return Result.Success(new UrlResponse { ShortCode = shortCode });
    }

    private async Task<Result<UrlResponse>> CreateNewShortUrlAsync(LongUrl longUrl, CancellationToken cancellationToken)
    {
        try
        {
            var snowflakeId = idGenerator.NextId();
            var shortCodeValue = base62Encoder.Encode(snowflakeId);

            if (string.IsNullOrWhiteSpace(shortCodeValue))
            {
                logger.LogError("Failed to generate short code for Snowflake ID: {SnowflakeId}", snowflakeId);
                return Result.Failure<UrlResponse>(UrlMappingErrors.CreationFailed());
            }

            var shortCode = ShortCode.Create(shortCodeValue);
            var urlMapping = UrlMapping.Create(snowflakeId, longUrl, shortCode);

            await dbContext.UrlMappings.AddAsync(urlMapping, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            _ = Task.Run(async () => await CacheShortCodeAsync(longUrl, shortCodeValue), cancellationToken);

            logger.LogInformation("Successfully created short URL for {LongUrl} with code {ShortCode}",
                longUrl.Value, shortCodeValue);

            return Result.Success(new UrlResponse { ShortCode = shortCodeValue });
        }
        catch (ShortCodeNotNullOrEmptyException ex)
        {
            logger.LogError(ex, "Invalid short code generated for URL: {LongUrl}", longUrl.Value);
            return Result.Failure<UrlResponse>(Error.Failure("ShortCode.Empty", ex.Message));
        }
        catch (ShortCodeInvalidFormatException ex)
        {
            logger.LogError(ex, "Invalid short code format generated for URL: {LongUrl}", longUrl.Value);
            return Result.Failure<UrlResponse>(Error.Failure("ShortCode.InvalidFormat", ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create short URL for {LongUrl}", longUrl.Value);
            return Result.Failure<UrlResponse>(UrlMappingErrors.CreationFailed());
        }
    }

    private async Task CacheShortCodeAsync(LongUrl longUrl, string shortCode)
    {
        try
        {
            var cacheKey = GenerateCacheKey(longUrl);
            await cacheService.SetStringAsync(cacheKey, shortCode, TimeSpan.FromDays(CacheExpirationDays));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cache short code for {LongUrl}", longUrl.Value);
        }
    }

    private static string GenerateCacheKey(LongUrl longUrl) => $"long:{longUrl.Value}";
}