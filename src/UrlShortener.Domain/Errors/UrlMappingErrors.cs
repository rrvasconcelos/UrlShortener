using UrlShortener.SharedKernel.Errors;

namespace UrlShortener.Domain.Errors;

public static class UrlMappingErrors
{
    //CreationFailed
    public static Error CreationFailed() => Error.Problem(
        "UrlMapping.CreationFailed",
        "Failed to create URL mapping.");
}