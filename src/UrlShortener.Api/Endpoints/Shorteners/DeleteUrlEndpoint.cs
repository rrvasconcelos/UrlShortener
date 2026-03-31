using UrlShortener.Api.Extensions;
using UrlShortener.Api.Infrastructure;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Shorteners.Delete;

namespace UrlShortener.Api.Endpoints.Shorteners;

public class DeleteUrlEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{shortCode}", HandleAsync)
            .RequireAuthorization();

    private static async Task<IResult> HandleAsync(
        string shortCode,
        ICommandHandler<DeleteUrlCommand> handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteUrlCommand(shortCode);
        var result = await handler.Handle(command, cancellationToken);

        return result.Match(
            () => Results.NoContent(),
            CustomResults.Problem);
    }
}
