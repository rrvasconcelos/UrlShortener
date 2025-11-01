namespace UrlShortener.Application.Abstractions.Cache;

public interface ICacheService
{
    Task<string?> GetStringAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> KeyExistsAsync(string key);
}
