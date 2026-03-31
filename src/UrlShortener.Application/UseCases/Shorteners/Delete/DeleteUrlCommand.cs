using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.UseCases.Shorteners.Delete;

public record DeleteUrlCommand(string ShortCode) : ICommand;
