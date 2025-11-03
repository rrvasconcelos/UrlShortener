using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Common;

namespace UrlShortener.Application.UseCases.Shorteners.GetByShortCode;

public record GetUrlQuery(string ShortCode) : IQuery<UrlResponse>;