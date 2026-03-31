namespace UrlShortener.Application.Abstractions.IdGeneration;

public interface IBase62Encoder
{
    string Encode(long value);
    long Decode(string encoded);
}
