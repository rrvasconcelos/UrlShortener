using UrlShortener.Application.Abstractions.IdGeneration;

namespace UrlShortener.Infrastructure.Services.CodeGeneration;

public class Base62Encoder : IBase62Encoder
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int Base = 62;

    public string Encode(long value)
    {
        if (value == 0)
            return Alphabet[0].ToString();

        var result = new Stack<char>();
        while (value > 0)
        {
            result.Push(Alphabet[(int)(value % Base)]);
            value /= Base;
        }
        return new string(result.ToArray());
    }

    public long Decode(string encoded)
    {
        long result = 0;
        foreach (var c in encoded)
        {
            var index = Alphabet.IndexOf(c);
            if (index < 0)
                throw new ArgumentException($"Invalid Base62 character: '{c}'", nameof(encoded));
            result = result * Base + index;
        }
        return result;
    }
}
