using FluentValidation;

namespace UrlShortener.Application.UseCases.Shorteners;

public class CreateShortUrlCommandValidator: AbstractValidator<CreateShortUrlCommand>
{
    public CreateShortUrlCommandValidator()
    {
        RuleFor(x => x.LongUrl)
            .NotNull()
            .WithMessage("The URL cannot be null.")
            .WithErrorCode("LongUrl.NotNull");

        RuleFor(x => x.LongUrl)
            .Must(BeAValidAbsoluteUrl)
            .When(x => x.LongUrl is not null)
            .WithMessage("The URL must be a valid absolute URL.")
            .WithErrorCode("LongUrl.InvalidFormat");
    }

    private static bool BeAValidAbsoluteUrl(Uri uri)
    {
        return uri.IsAbsoluteUri && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}