using Frete.Application.Services;
using Frete.Application.Strategies;
using Frete.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Frete.Tests;

public class FreteStrategyResolverTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FreteStrategyResolver _freteStrategyResolver;
    
    public FreteStrategyResolverTests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<NormalFreteStrategy>();
        services.AddScoped<ExpressaFreteStrategy>();
        services.AddScoped<AgendadaFreteStrategy>();
        
        _serviceProvider = services.BuildServiceProvider();
        _freteStrategyResolver = new FreteStrategyResolver(_serviceProvider);
    }

    [Fact]
    public void Resolver_deve_retornar_NormalFreteStrategy()
    {
        // Act
        var strategy = _freteStrategyResolver.Resolve(ModalidadeFrete.Normal);
        
        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<NormalFreteStrategy>(strategy);
    }
    
    [Fact]
    public void Resolver_deve_retornar_ExpressaFreteStrategy()
    {
        // Act
        var strategy = _freteStrategyResolver.Resolve(ModalidadeFrete.Expressa);
        
        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<ExpressaFreteStrategy>(strategy);
    }
    
    [Fact]
    public void Resolver_deve_retornar_AgendadaFreteStrategy()
    {
        // Act
        var strategy = _freteStrategyResolver.Resolve(ModalidadeFrete.Agendada);
        
        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<AgendadaFreteStrategy>(strategy);
    }
}