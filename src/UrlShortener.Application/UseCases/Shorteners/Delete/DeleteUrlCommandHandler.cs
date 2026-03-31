using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners.Delete;

internal sealed class DeleteUrlCommandHandler(
    IApplicationDbContext dbContext,
    ICacheService cacheService,
    ILogger<DeleteUrlCommandHandler> logger)
    : ICommandHandler<DeleteUrlCommand>
{
    public async Task<Result> Handle(DeleteUrlCommand command, CancellationToken cancellationToken)
    {
        var urlMapping = await dbContext.UrlMappings
            .FirstOrDefaultAsync(m => m.ShortCode != null && m.ShortCode.Value == command.ShortCode, cancellationToken);

        if (urlMapping is null)
        {
            logger.LogWarning("URL mapping not found for short code: {ShortCode}", command.ShortCode);
            return Result.Failure(Error.NotFound("UrlMapping.NotFound", "URL mapping not found"));
        }

        dbContext.UrlMappings.Remove(urlMapping);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await cacheService.RemoveAsync($"short:{command.ShortCode}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove cache for short code: {ShortCode}", command.ShortCode);
        }

        logger.LogInformation("Deleted URL mapping for short code: {ShortCode}", command.ShortCode);
        return Result.Success();
    }
}
