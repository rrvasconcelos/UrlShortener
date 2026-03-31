using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Domain.Entities;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Analytics;

internal sealed class RegisterClickCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<RegisterClickCommandHandler> logger)
    : ICommandHandler<RegisterClickCommand>
{
    public async Task<Result> Handle(RegisterClickCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var click = UrlClick.Create(
                command.ShortCode,
                command.Country,
                command.DeviceType,
                command.IpHash,
                command.Referrer);

            await dbContext.UrlClicks.AddAsync(click, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to register click for short code: {ShortCode}", command.ShortCode);
            return Result.Success(); // Don't fail the redirect if click tracking fails
        }
    }
}
