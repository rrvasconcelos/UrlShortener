using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Entities;

namespace UrlShortener.Domain.Entities;

public class UrlMapping : Entity<long>
{
    public LongUrl LongUrl { get; private set; } = default!;
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

    private UrlMapping(long id, LongUrl longUrl, ShortCode shortCode, DateTime? expiresAt = null)
    {
        Id = id;
        LongUrl = longUrl;
        ShortCode = shortCode;
        ExpiresAt = expiresAt;
    }

    public static UrlMapping Create(LongUrl longUrl, DateTime? expiresAt = null)
    {
        return new UrlMapping(longUrl, expiresAt);
    }

    public static UrlMapping Create(long id, LongUrl longUrl, ShortCode shortCode, DateTime? expiresAt = null)
    {
        return new UrlMapping(id, longUrl, shortCode, expiresAt);
    }

    public void AddShortCode(ShortCode shortCode) => ShortCode = shortCode;

    public void IncrementClickCount() => ClickCount++;
}

