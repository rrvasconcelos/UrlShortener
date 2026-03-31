using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.IdGeneration;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Infrastructure.Database;
using UrlShortener.Infrastructure.Identity;
using UrlShortener.Infrastructure.Services.Auth;
using UrlShortener.Infrastructure.Services.Cache;
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
        .AddIdentityServices()
        .AddAuthServices(configuration)
        .AddHealthChecks(configuration)
        .AddServices(configuration);

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

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<UrlShortenerDbContext>());

        return services;
    }

    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UrlShortenerDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    private static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
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

        services.Configure<SnowflakeSettings>(configuration.GetSection("SnowflakeSettings"));
        services.AddSingleton<ISnowflakeIdGenerator, SnowflakeIdGenerator>();
        services.AddSingleton<IBase62Encoder, Base62Encoder>();

        var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("A string de conexão do Redis não foi encontrada no appsettings.json.");
        }

        var redisConnection = ConfigurationOptions.Parse(redisConnectionString);

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
