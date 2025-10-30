using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Infrastructure.Database.Mappings;

public class UrlMappingConfiguration : IEntityTypeConfiguration<UrlMapping>
{
    public void Configure(EntityTypeBuilder<UrlMapping> builder)
    {
        // Tabela
        builder.ToTable("UrlMappings");

        // 1. Chave Primária
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        // 2. Propriedades Obrigatórias e Tamanhos (Value Objects via conversion)
        builder
            .Property(u => u.LongUrl)
            .HasConversion(
                v => v.Value,           // to provider (string)
                v => LongUrl.Create(v)  // from provider
            )
            .HasMaxLength(2048)
            .IsRequired()
            .HasColumnName("LongUrl");

        builder
            .Property(u => u.ShortCode)
            .HasConversion(
                v => v.Value,             // to provider (string)
                v => ShortCode.Create(v)  // from provider
            )
            .HasMaxLength(7)
            .IsRequired()
            .HasColumnName("ShortCode");

        // 3. Índice Único para ShortCode
        builder
            .HasIndex(u => u.ShortCode)
            .IsUnique();

        // 4. Propriedades de Analytics e Tempo
        builder
            .Property(u => u.ClickCount)
            .IsRequired()
            .HasDefaultValue(0L);

        builder
            .Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()"); // PostgreSQL now()

        builder
            .Property(u => u.ExpiresAt); // Nullable por padrão
    }
}
