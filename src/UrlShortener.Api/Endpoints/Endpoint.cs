using UrlShortener.Api.Endpoints.Analytics;
using UrlShortener.Api.Endpoints.Auth;
using UrlShortener.Api.Endpoints.Shorteners;

namespace UrlShortener.Api.Endpoints;

public static class Endpoint
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("");

        endpoints.MapGroup("v1/shorten")
            .WithTags("Shorteners")
            .RequireRateLimiting("anonymous")
            .MapEndpoint<CreateShortUrlEndpoint>()
            .MapEndpoint<GetByShortCodeEndpoint>()
            .MapEndpoint<DeleteUrlEndpoint>();

        endpoints.MapGroup("v1/analytics")
            .WithTags("Analytics")
            .MapEndpoint<GetAnalyticsEndpoint>();

        endpoints.MapGroup("api/auth")
            .WithTags("Auth")
            .MapEndpoint<AuthEndpoints>();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}
