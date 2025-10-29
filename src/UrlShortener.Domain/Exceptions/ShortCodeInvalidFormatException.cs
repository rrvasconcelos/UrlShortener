using UrlShortener.SharedKernel.Exceptions;

namespace UrlShortener.Domain.Exceptions;

public class ShortCodeInvalidFormatException() : DomainException("Short code format is invalid. It must be base62 (A-Z, a-z, 0-9) with max 7 characters.");
