using UrlShortener.SharedKernel.Entities;

namespace UrlShortener.Domain.Entities;

public class UrlClick : Entity<long>
{
    public string ShortCode { get; private set; } = string.Empty;
    public DateTime ClickedAt { get; private set; }
    public string? Country { get; private set; }
    public string? DeviceType { get; private set; }
    public string? IpHash { get; private set; }
    public string? Referrer { get; private set; }

    private UrlClick() { }

    public static UrlClick Create(string shortCode, string? country, string? deviceType, string? ipHash, string? referrer)
    {
        return new UrlClick
        {
            ShortCode = shortCode,
            ClickedAt = DateTime.UtcNow,
            Country = country,
            DeviceType = deviceType,
            IpHash = ipHash,
            Referrer = referrer
        };
    }
}
