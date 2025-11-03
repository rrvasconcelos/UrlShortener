using UrlShortener.SharedKernel.Exceptions;

namespace UrlShortener.Domain.Exceptions;

public class ShortCodeNotNullOrEmptyException() : DomainException("Short code cannot be null or empty.");
