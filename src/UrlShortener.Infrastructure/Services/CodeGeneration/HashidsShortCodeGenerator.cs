using HashidsNet;
using UrlShortener.Application.Abstractions.ShortCode;

namespace UrlShortener.Infrastructure.Services.CodeGeneration;

public class HashidsShortCodeGenerator : IShortCodeGenerator
{
    private readonly Hashids _hashids;

    // Evite hardcode: injete o salt e o comprimento mínimo via DI/configuração
    public HashidsShortCodeGenerator(string salt, int minHashLength = 7)
    {
        if (string.IsNullOrWhiteSpace(salt))
        {
            throw new ArgumentException("Salt must be provided and not be empty.", nameof(salt));
        }

        if (minHashLength <= 0)
        {
            minHashLength = 7;
        }

        _hashids = new Hashids(salt, minHashLength);
    }

    public string Encode(long id)
    {
        return _hashids.EncodeLong(id);
    }

    public long Decode(string shortCode)
    {
        var ids = _hashids.DecodeLong(shortCode);

        if (ids.Length == 0)
        {
            throw new InvalidShortCodeException(shortCode);
        }

        return ids[0];
    }
}
