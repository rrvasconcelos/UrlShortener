using UrlShortener.Application.Abstractions.Messaging;

namespace UrlShortener.Application.UseCases.Analytics;

public record RegisterClickCommand : ICommand
{
    public required string ShortCode { get; init; }
    public string? Country { get; init; }
    public string? DeviceType { get; init; }
    public string? IpHash { get; init; }
    public string? Referrer { get; init; }
}
