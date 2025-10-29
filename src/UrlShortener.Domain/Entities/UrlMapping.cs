namespace UrlShortener.Domain.Entities;
public class UrlMapping
{
    public long Id { get; private set; }
    public string LongUrl { get; private set; }
    public string ShortCode { get; private set; }
    public long ClickCount { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }

    private UrlMapping()
    {
        
    }

    private UrlMapping(string longUrl, string shortCode, DateTime? expiresAt = null)
    {
        LongUrl = longUrl;
        ShortCode = shortCode;
        ExpiresAt = expiresAt;
    }

    public static UrlMapping Create(string longUrl, string shortCode, DateTime? expiresAt = null)
    {
        return new UrlMapping(longUrl, shortCode, expiresAt);
    }
}

