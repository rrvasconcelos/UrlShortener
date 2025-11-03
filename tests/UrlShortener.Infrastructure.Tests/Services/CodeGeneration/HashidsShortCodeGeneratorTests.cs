using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Infrastructure.Services.CodeGeneration;

namespace UrlShortener.Infrastructure.Tests.Services.CodeGeneration;

public class HashidsShortCodeGeneratorTests : ShortCodeGeneratorContractTests
{
    protected override IShortCodeGenerator CreateSut() => new HashidsShortCodeGenerator("unit_test_salt", 7);
}
