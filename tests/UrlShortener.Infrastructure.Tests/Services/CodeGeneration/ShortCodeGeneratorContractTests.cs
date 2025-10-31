using System;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Infrastructure.Services.CodeGeneration;

namespace UrlShortener.Infrastructure.Tests.Services.CodeGeneration;

// Testes de "contrato" para qualquer implementação de IShortCodeGenerator.
public abstract class ShortCodeGeneratorContractTests
{
    // Cada implementação concreta fornece o SUT via fábrica
    protected abstract IShortCodeGenerator CreateSut();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(1234567890)]
    public void EncodeThenDecode_RoundTrip(long id)
    {
        var sut = CreateSut();

        var code = sut.Encode(id);
        var decoded = sut.Decode(code);

        Assert.Equal(id, decoded);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("inv@lid")]
    [InlineData("aaaaaaaaaaaa")]
    public void Decode_InvalidCode_ThrowsInvalidShortCodeException(string shortCode)
    {
        var sut = CreateSut();

        Assert.Throws<InvalidShortCodeException>(() => sut.Decode(shortCode));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1234)]
    public void Encode_MinLength_IsRespected(long id)
    {
        var sut = CreateSut();

        var code = sut.Encode(id);

        // A implementação atual usa MinHashLength = 7
        Assert.True(code.Length >= 7, $"Expected code length >= 7, got '{code}' ({code.Length}).");
    }

    [Fact]
    public void Encode_IsDeterministic_ForSameId()
    {
        var sut = CreateSut();

        var code1 = sut.Encode(12345);
        var code2 = sut.Encode(12345);

        Assert.Equal(code1, code2);
    }

    [Fact]
    public void Encode_DifferentIds_ProduceDifferentCodes()
    {
        var sut = CreateSut();

        var code1 = sut.Encode(1);
        var code2 = sut.Encode(2);

        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void Decode_Null_ThrowsException()
    {
        var sut = CreateSut();

        // Implementações podem propagar exceções diferentes ao receber null
        // (ex.: NullReferenceException/ArgumentNullException do HashidsNet).
        // O contrato aqui exige que null NÃO seja aceito e gere uma exceção.
        Assert.ThrowsAny<Exception>(() => sut.Decode(null!));
    }
}
