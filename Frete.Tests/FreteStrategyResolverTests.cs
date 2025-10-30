using Frete.Application.Services;
using Frete.Application.Strategies;
using Frete.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

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
        var mockLogger = new Mock<ILogger<FreteStrategyResolver>>();
        _freteStrategyResolver = new FreteStrategyResolver(_serviceProvider, mockLogger.Object);
    }

    [Fact]
    public void Resolve_ShouldReturnNormalFreteStrategy_WhenModalidadeIsNormal()
    {
        // Act
        var strategy = _freteStrategyResolver.Resolve(ModalidadeFrete.Normal);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<NormalFreteStrategy>(strategy);
    }

    [Fact]
    public void Resolve_ShouldReturnExpressaFreteStrategy_WhenModalidadeIsExpressa()
    {
        // Act
        var strategy = _freteStrategyResolver.Resolve(ModalidadeFrete.Expressa);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<ExpressaFreteStrategy>(strategy);
    }

    [Fact]
    public void Resolve_ShouldReturnAgendadaFreteStrategy_WhenModalidadeIsAgendada()
    {
        // Act
        var strategy = _freteStrategyResolver.Resolve(ModalidadeFrete.Agendada);

        // Assert
        Assert.NotNull(strategy);
        Assert.IsType<AgendadaFreteStrategy>(strategy);
    }
}