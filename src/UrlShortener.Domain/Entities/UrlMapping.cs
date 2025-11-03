using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Entities;

namespace UrlShortener.Domain.Entities;

public class UrlMapping : Entity<long>
{
    public LongUrl LongUrl { get; private set; }
    public ShortCode? ShortCode { get; private set; }
    public long ClickCount { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }

    private UrlMapping()
    {

    }

    private UrlMapping(LongUrl longUrl, DateTime? expiresAt = null)
    {
        LongUrl = longUrl;
        ExpiresAt = expiresAt;
    }

    public static UrlMapping Create(LongUrl longUrl, DateTime? expiresAt = null)
    {
        return new UrlMapping(longUrl, expiresAt);
    }

    public void AddShortCode(ShortCode shortCode) => ShortCode = shortCode;
}

