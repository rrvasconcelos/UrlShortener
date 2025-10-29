using UrlShortener.SharedKernel.Exceptions;

namespace UrlShortener.Domain.Exceptions;

public class LongUrlInvalidFormatException() : DomainException("Long URL format is invalid.");