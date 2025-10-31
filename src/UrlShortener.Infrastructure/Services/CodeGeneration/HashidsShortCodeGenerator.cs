using HashidsNet;
using UrlShortener.Application.Abstractions.ShortCode;

namespace UrlShortener.Infrastructure.Services.CodeGeneration;

public class HashidsShortCodeGenerator : IShortCodeGenerator
{
    private readonly Hashids _hashids;
    private const string Salt = "sua_chave_secreta_e_longa_aqui_para_seguranca";
    private const int MinHashLength = 7;

    public HashidsShortCodeGenerator()
    {
        _hashids = new Hashids(Salt, MinHashLength);
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
