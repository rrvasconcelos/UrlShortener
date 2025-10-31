using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners;

public class CreateShortUrlCommandHandler(
    IShortCodeGenerator codeGenerator,
    IApplicationDbContext dbContext,
    ILogger<CreateShortUrlCommandHandler> logger)
    : ICommandHandler<CreateShortUrlCommand, UrlResponse>
{
    public async Task<Result<UrlResponse>> Handle(CreateShortUrlCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.LongUrl.ToString()))
            return Result.Failure<UrlResponse>(Error.Problem("ShortUrl.NotNull", "A URL longa não pode ser vazia."));

        var longUrl = LongUrl.Create(command.LongUrl.ToString());
        
        var urlMapping = UrlMapping.Create(longUrl);

        await dbContext.UrlMappings.AddAsync(urlMapping, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        var shortCode = codeGenerator.Encode(urlMapping.Id);
        
        urlMapping.AddShortCode(ShortCode.Create(shortCode));
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UrlResponse
        {
            ShortCode = urlMapping.ShortCode?.Value ?? shortCode,
            LongUrl = urlMapping.LongUrl.Value,
            ClickCount = urlMapping.ClickCount
        };
    }
}