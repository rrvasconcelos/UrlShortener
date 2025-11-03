using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Database;

public class UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
: DbContext(options), IApplicationDbContext
{
    public DbSet<UrlMapping> UrlMappings => Set<UrlMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de IEntityTypeConfiguration deste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UrlShortenerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
