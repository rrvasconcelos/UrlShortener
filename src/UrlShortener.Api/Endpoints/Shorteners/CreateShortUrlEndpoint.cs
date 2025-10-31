using System;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Shorteners;

namespace UrlShortener.Api.Endpoints.Shorteners;

public record Request(Uri LongUrl);

public class CreateShortUrlEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
       => app.MapPost("/", HandleAsync)
            .Produces<long>();

    private static async Task<IResult> HandleAsync(
        Request  request,
        ICommandHandler<CreateShortUrlCommand, UrlResponse>  handler, 
        CancellationToken cancellationToken = default )
    {
        var command = new CreateShortUrlCommand
        {
            LongUrl = request.LongUrl
        };
        
        var response = await handler.Handle(command, cancellationToken);
        
        return Results.Ok(response);
    }
}
