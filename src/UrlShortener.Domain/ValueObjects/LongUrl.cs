using UrlShortener.Domain.Exceptions;
using UrlShortener.SharedKernel.ValueObjects;

namespace UrlShortener.Domain.ValueObjects;
public record LongUrl : ValueObject
{
    public string Value { get; }

    private LongUrl(string value)
    {
        Value = value;
    }

    public static LongUrl Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new LongUrlNotNullOrEmptyException();
        }

        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
        {
            throw new LongUrlInvalidFormatException();
        }

        return new LongUrl(value);
    }
}

