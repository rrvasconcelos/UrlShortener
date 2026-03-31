using System.Security.Cryptography;
using System.Text;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Infrastructure;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.UseCases.Analytics;
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
        ICommandHandler<RegisterClickCommand> clickHandler,
        IHostEnvironment environment,
        HttpContext httpContext,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUrlQuery(shortCode);

        var result = await handler.Handle(query, cancellationToken);

        return result.Match(
            success =>
            {
                if (string.IsNullOrWhiteSpace(success.LongUrl))
                    return Results.NotFound("URL not found");

                _ = Task.Run(async () =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                    var ipHash = ip is not null ? HashIp(ip) : null;
                    var userAgent = httpContext.Request.Headers.UserAgent.ToString();
                    var deviceType = DetectDeviceType(userAgent);
                    var referrer = httpContext.Request.Headers.Referer.ToString();

                    var clickCommand = new RegisterClickCommand
                    {
                        ShortCode = shortCode,
                        IpHash = ipHash,
                        DeviceType = deviceType,
                        Referrer = string.IsNullOrWhiteSpace(referrer) ? null : referrer
                    };
                    await clickHandler.Handle(clickCommand, CancellationToken.None);
                });

                return CreateCacheableRedirect(success.LongUrl, environment.IsDevelopment());
            },
            CustomResults.Problem);
    }

    private static string HashIp(string ip)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string DetectDeviceType(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "unknown";

        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return "mobile";
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "tablet";
        return "desktop";
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
