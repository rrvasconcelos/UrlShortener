using UrlShortener.SharedKernel.Exceptions;

namespace UrlShortener.Domain.Exceptions;

public class LongUrlNotNullOrEmptyException : DomainException
{
    public LongUrlNotNullOrEmptyException()
        : base("Long URL cannot be null or empty.")
    {
    }
}

public class LongUrlInvalidFormatException : DomainException
{
    public LongUrlInvalidFormatException()
        : base("Long URL format is invalid.")
    {
    }
}

