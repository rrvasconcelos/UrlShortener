using UrlShortener.Api.Extensions;
using UrlShortener.Api.Infrastructure;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Analytics;

namespace UrlShortener.Api.Endpoints.Analytics;

public class GetAnalyticsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{shortCode}", HandleAsync)
            .Produces<UrlAnalyticsResponse>()
            .RequireAuthorization();

    private static async Task<IResult> HandleAsync(
        string shortCode,
        IQueryHandler<GetUrlAnalyticsQuery, UrlAnalyticsResponse> handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUrlAnalyticsQuery(shortCode);
        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            success => Results.Ok(success),
            CustomResults.Problem);
    }
}
