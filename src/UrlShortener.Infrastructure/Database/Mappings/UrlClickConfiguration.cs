using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Database.Mappings;

public class UrlClickConfiguration : IEntityTypeConfiguration<UrlClick>
{
    public void Configure(EntityTypeBuilder<UrlClick> builder)
    {
        builder.ToTable("UrlClicks");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.ShortCode).IsRequired().HasMaxLength(12);
        builder.Property(u => u.ClickedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(u => u.Country).HasMaxLength(100);
        builder.Property(u => u.DeviceType).HasMaxLength(50);
        builder.Property(u => u.IpHash).HasMaxLength(64);
        builder.Property(u => u.Referrer).HasMaxLength(2048);

        builder.HasIndex(u => u.ShortCode);
    }
}
