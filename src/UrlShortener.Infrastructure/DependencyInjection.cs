using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Infrastructure.Database;
using UrlShortener.Infrastructure.Services.CodeGeneration;

namespace UrlShortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    => services
        .AddDataBase(configuration, isDevelopment)
        .AddHealthChecks(configuration)
        .AddServices();

    private static IServiceCollection AddDataBase(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        services.AddDbContext<UrlShortenerDbContext>((serviceProvider, options) =>
        {
            var connection = configuration.GetConnectionString("Database");

            options.UseNpgsql(
                connection,
                b => b.MigrationsAssembly(typeof(UrlShortenerDbContext).Assembly.FullName));

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();

                var logger = serviceProvider.GetRequiredService<ILogger<UrlShortenerDbContext>>();
                options.LogTo((message) => logger.LogInformation(message),
                    [DbLoggerCategory.Database.Command.Name],
                    LogLevel.Information);
            }
        });

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IShortCodeGenerator>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var salt = config["Hashids:Salt"];
            var minLen = config.GetValue<int?>("Hashids:MinHashLength") ?? 7;
            if (string.IsNullOrWhiteSpace(salt))
            {
                throw new InvalidOperationException("Hashids:Salt is not configured. Set it via user-secrets, environment variables, or a secrets vault.");
            }
            return new HashidsShortCodeGenerator(salt, minLen);
        });

        return services;
    }

}
