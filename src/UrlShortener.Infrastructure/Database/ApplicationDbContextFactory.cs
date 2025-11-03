using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UrlShortener.Infrastructure.Database;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<UrlShortenerDbContext>
{
    public UrlShortenerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UrlShortenerDbContext>();

        var connectionString = "Host=localhost;Port=5432;Database=urlshortener_db;Username=postgres;Password=postgres;Include Error Detail=true";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql =>
            {
                var migrationsAssembly = typeof(UrlShortenerDbContext).Assembly.GetName().Name!;
                npgsql.MigrationsAssembly(migrationsAssembly);
            });

        return new UrlShortenerDbContext(optionsBuilder.Options);
    }
}
