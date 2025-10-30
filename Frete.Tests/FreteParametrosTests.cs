using Frete.Domain.Exceptions;
using Frete.Domain.ValueObjects;
using Xunit;

namespace Frete.Tests;

public class FreteParametrosTests
{
    [Theory]
    [InlineData(10, 20, 5)]
    [InlineData(0.5, 100, 0)]
    [InlineData(1, 1, 1)]
    public void Constructor_ShouldCreateValidFreteParametros(decimal pesoKg, decimal distanciaKm, decimal taxaFixa)
    {
        // Act
        var parametros = new FreteParametros(pesoKg, distanciaKm, taxaFixa);

        // Assert
        Assert.Equal(pesoKg, parametros.PesoKg);
        Assert.Equal(distanciaKm, parametros.DistanciaKm);
        Assert.Equal(taxaFixa, parametros.TaxaFixa);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ShouldThrowInvalidFreteParametrosException_WhenPesoKgIsInvalid(decimal pesoKgInvalido)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidFreteParametrosException>(() =>
            new FreteParametros(pesoKgInvalido, 10, 5));
        Assert.Equal("O peso deve ser maior que zero.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ShouldThrowInvalidFreteParametrosException_WhenDistanciaKmIsInvalid(decimal distanciaKmInvalida)
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidFreteParametrosException>(() =>
            new FreteParametros(10, distanciaKmInvalida, 5));
        Assert.Equal("A distância deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidFreteParametrosException_WhenTaxaFixaIsNegative()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidFreteParametrosException>(() =>
            new FreteParametros(10, 20, -5));
        Assert.Equal("A taxa fixa não pode ser negativa.", exception.Message);
    }
}
