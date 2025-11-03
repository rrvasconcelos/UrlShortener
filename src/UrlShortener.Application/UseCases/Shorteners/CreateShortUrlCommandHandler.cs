using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Errors;
using UrlShortener.Domain.Exceptions;
using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners;

public class CreateShortUrlCommandHandler(
    IShortCodeGenerator codeGenerator,
    IApplicationDbContext dbContext,
    ICacheService cacheService,
    ILogger<CreateShortUrlCommandHandler> logger)
    : ICommandHandler<CreateShortUrlCommand, UrlResponse>
{
    private const int CacheExpirationDays = 30;
    
    public async Task<Result<UrlResponse>> Handle(CreateShortUrlCommand command, CancellationToken cancellationToken)
    {
        var longUrl = LongUrl.Create(command.LongUrl.ToString());
        
        // Verifica cache primeiro
        var cacheResult = await TryGetFromCacheAsync(longUrl);
        if (cacheResult is not null)
        {
            return cacheResult;
        }

        // Verifica banco de dados
        var dbResult = await TryGetFromDatabaseAsync(longUrl, cancellationToken);
        if (dbResult is not null)
        {
            return dbResult;
        }

        // Cria nova URL curta
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
            return null; // Continue sem cache em caso de erro
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
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var urlMapping = UrlMapping.Create(longUrl);
            await dbContext.UrlMappings.AddAsync(urlMapping, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            var shortCodeValue = codeGenerator.Encode(urlMapping.Id);
            if (string.IsNullOrWhiteSpace(shortCodeValue))
            {
                logger.LogError("Failed to generate short code for URL mapping ID: {UrlMappingId}", urlMapping.Id);
                return Result.Failure<UrlResponse>(UrlMappingErrors.CreationFailed());
            }

            var shortCode = ShortCode.Create(shortCodeValue);
            urlMapping.AddShortCode(shortCode);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Cache de forma assíncrona após commit
            _ = Task.Run(async () => await CacheShortCodeAsync(longUrl, shortCodeValue), cancellationToken);

            logger.LogInformation("Successfully created short URL for {LongUrl} with code {ShortCode}", 
                longUrl.Value, shortCodeValue);

            return Result.Success(new UrlResponse { ShortCode = shortCodeValue });
        }
        catch (ShortCodeNotNullOrEmptyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Invalid short code generated for URL: {LongUrl}", longUrl.Value);
            return Result.Failure<UrlResponse>(Error.Failure("ShortCode.Empty", ex.Message));
        }
        catch (ShortCodeInvalidFormatException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Invalid short code format generated for URL: {LongUrl}", longUrl.Value);
            return Result.Failure<UrlResponse>(Error.Failure("ShortCode.InvalidFormat", ex.Message));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Failed to create short URL for {LongUrl}", longUrl.Value);
            return Result.Failure<UrlResponse>(UrlMappingErrors.CreationFailed());
        }
    }

    private async Task CacheShortCodeAsync(LongUrl longUrl, string shortCode)
    {
        try
        {
            var cacheKey = GenerateCacheKey(longUrl);
            var expiration = TimeSpan.FromDays(CacheExpirationDays);
            await cacheService.SetStringAsync(cacheKey, shortCode, expiration);
        }
        catch (Exception ex)
        {
            // Log mas não falha a operação principal
            logger.LogWarning(ex, "Failed to cache short code for {LongUrl}", longUrl.Value);
        }
    }

    private static string GenerateCacheKey(LongUrl longUrl) => $"long:{longUrl.Value}";
}