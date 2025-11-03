using System;
using UrlShortener.Api.Endpoints.Shorteners;

namespace UrlShortener.Api.Endpoints;

public static class Endpoint
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app
            .MapGroup("");

        endpoints.MapGroup("v1/shorten")
            .WithTags("Restaurants")
            .MapEndpoint<CreateShortUrlEndpoint>()
            .MapEndpoint<GetByShortCodeEndpoint>();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}
