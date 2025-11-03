using UrlShortener.Api.Extensions;
using UrlShortener.Api.Infrastructure;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Common;
using UrlShortener.Application.UseCases.Shorteners.GetByShortCode;

namespace UrlShortener.Api.Endpoints.Shorteners;

public class GetByShortCodeEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
   => app.MapGet("/{shortCode}", HandleAsync)
        .Produces<UrlResponse>();

    private static async Task<IResult> HandleAsync(
        string shortCode,
        IQueryHandler<GetUrlQuery, UrlResponse> handler,
        IHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUrlQuery(shortCode);

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            success => string.IsNullOrWhiteSpace(success.LongUrl) 
                ? Results.NotFound("URL not found") 
                : CreateCacheableRedirect(success.LongUrl, environment.IsDevelopment()),
            CustomResults.Problem);
    }

    private static IResult CreateCacheableRedirect(string url, bool isDevelopment)
    {
        return new CacheableRedirectResult(url, isDevelopment);
    }
}

public class CacheableRedirectResult(string url, bool isDevelopment = false) : IResult
{
    private readonly string _url = url;
    private readonly bool _isDevelopment = isDevelopment;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        // Define cache baseado no ambiente
        if (_isDevelopment)
        {
            // Em desenvolvimento: cache menor para facilitar testes (5 minutos)
            var expiresAt = DateTime.UtcNow.AddMinutes(5);
            httpContext.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromMinutes(5)
            };
            httpContext.Response.GetTypedHeaders().Expires = expiresAt;
        }
        else
        {
            // Em produção: cache longo (1 ano) + immutable
            var expiresAt = DateTime.UtcNow.AddYears(1);
            var cacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(365)
            };
            
            // Define o cache control primeiro
            httpContext.Response.GetTypedHeaders().CacheControl = cacheControl;
            
            // Adiciona 'immutable' usando Append (mais seguro que concatenação)
            httpContext.Response.Headers.Append("Cache-Control", "immutable");
            
            httpContext.Response.GetTypedHeaders().Expires = expiresAt;
        }
        
        // Executa o redirecionamento 301
        httpContext.Response.StatusCode = 301;
        httpContext.Response.GetTypedHeaders().Location = new Uri(_url);
        
        await Task.CompletedTask;
    }
}
