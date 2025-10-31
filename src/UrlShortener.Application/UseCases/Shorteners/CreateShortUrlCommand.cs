using System;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.SharedKernel.Errors;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners;

public record CreateShortUrlCommand : ICommand<UrlResponse>
{
    public required Uri LongUrl { get; init; }
}


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

        throw new NotImplementedException();
    }
}


public record UrlResponse
{
    public required string ShortCode { get; set; }
    public required string LongUrl { get; set; }
    public long ClickCount { get; set; }
}