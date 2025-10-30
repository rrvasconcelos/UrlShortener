using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Database;

public class UrlShortenerDbContext : DbContext
{
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options)
        : base(options)
    {
    }

    public DbSet<UrlMapping> UrlMappings => Set<UrlMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações de IEntityTypeConfiguration deste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UrlShortenerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
