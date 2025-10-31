using System;
using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.UseCases.Shorteners;

public record CreateShortUrlCommand : ICommand<UrlResponse>
{
    public required Uri LongUrl { get; init; }
}