using UrlShortener.SharedKernel.Exceptions;

namespace UrlShortener.Infrastructure.Services.CodeGeneration;

public class InvalidShortCodeException(string shortCode)
: DomainException($"The short code '{shortCode}' is invalid.")
{
}
