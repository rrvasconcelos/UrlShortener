using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using UrlShortener.Application.Abstractions.Data;

namespace UrlShortener.Api.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<global::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IApplicationDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database for testing
            services.AddDbContext<IApplicationDbContext, TestApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });
        });

        builder.UseEnvironment("Testing");
    }
}

// Simple test DbContext implementation
public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UrlShortener.Domain.Entities.UrlMapping> UrlMappings { get; set; } = null!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Simple configuration for testing
        modelBuilder.Entity<UrlShortener.Domain.Entities.UrlMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LongUrl)
                .HasConversion(
                    v => v.Value,
                    v => UrlShortener.Domain.ValueObjects.LongUrl.Create(v));
            entity.Property(e => e.ShortCode)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? UrlShortener.Domain.ValueObjects.ShortCode.Create(v) : null);
        });

        base.OnModelCreating(modelBuilder);
    }
}