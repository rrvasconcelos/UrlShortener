using UrlShortener.Api.Infrastructure;

namespace UrlShortener.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        // Remover Swagger por enquanto - problema de compatibilidade com .NET 10
        // services.AddSwaggerGen();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}