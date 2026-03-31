using System.Text.RegularExpressions;
using UrlShortener.Domain.Exceptions;
using UrlShortener.SharedKernel.ValueObjects;

namespace UrlShortener.Domain.ValueObjects;

public partial record ShortCode : ValueObject
{
    public string Value { get; }

    private ShortCode(string value)
    {
        Value = value;
    }

    public static ShortCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ShortCodeNotNullOrEmptyException();
        }

        if (!MyRegex().IsMatch(value))
        {
            throw new ShortCodeInvalidFormatException();
        }

        return new ShortCode(value);
    }


    [GeneratedRegex("^[A-Za-z0-9]{1,12}$")]
    private static partial Regex MyRegex();
}
