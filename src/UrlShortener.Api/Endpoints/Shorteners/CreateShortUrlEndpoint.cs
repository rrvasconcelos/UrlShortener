using System;

namespace UrlShortener.Api.Endpoints.Shorteners;

public class CreateShortUrlEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
       => app.MapPost("/", HandleAsync)
            .Produces<long>();

    private static async Task<IResult> HandleAsync()
    {
        // Lógica do endpoint para criar um URL curto
        throw new NotImplementedException();
    }
}
