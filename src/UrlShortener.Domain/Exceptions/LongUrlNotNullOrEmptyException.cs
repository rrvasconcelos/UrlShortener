using UrlShortener.SharedKernel.Exceptions;

namespace UrlShortener.Domain.Exceptions;

public class LongUrlNotNullOrEmptyException() : DomainException("Long URL cannot be null or empty.");