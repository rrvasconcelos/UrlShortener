using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Common;

namespace UrlShortener.Application.UseCases.Shorteners.Create;

public record CreateShortUrlCommand : ICommand<UrlResponse>
{
    public required Uri LongUrl { get; init; }
}