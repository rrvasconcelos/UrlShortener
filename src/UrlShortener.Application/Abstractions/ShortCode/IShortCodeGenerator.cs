using System;

namespace UrlShortener.Application.Abstractions.ShortCode;

public interface IShortCodeGenerator
{
    string Encode(long id);
    long Decode(string shortCode);
}
