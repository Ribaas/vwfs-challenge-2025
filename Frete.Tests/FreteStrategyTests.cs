using Frete.Application.Strategies;
using Frete.Domain.ValueObjects;

namespace Frete.Tests;

public class FreteStrategyTests
{
    [Fact]
    public void Deve_calcular_frete_normal()
    {
        // Arrange
        var strategy = new NormalFreteStrategy();
        // parametros = peso, distancia, taxa fixa
        var parametros = new FreteParametros(5m, 10m, 2m);
        
        // Act
        var valor = strategy.CalcularFrete(parametros);
        
        // Assert
        // Calculo Frete Normal = peso * 0.5 + distancia * 0.1 + taxaFixa (2)
        var esperado = (5m * 0.5m) + (10m * 0.1m) + 2m;
        Assert.Equal(esperado, valor);
    }
    
    [Fact]
    public void Deve_calcular_frete_expresso()
    {
        // Arrange
        var strategy = new ExpressaFreteStrategy();
        // parametros = peso, distancia, taxa fixa
        var parametros = new FreteParametros(5m, 10m, 2m);
        
        // Act
        var valor = strategy.CalcularFrete(parametros);
        
        // Assert
        // Calculo Frete Expresso = peso * 0.5 + distancia * 1 + taxaFixa (5)
        var esperado = (5m * 0.5m) + (10m * 1m) + 5m;
        Assert.Equal(esperado, valor);
    }
    
}