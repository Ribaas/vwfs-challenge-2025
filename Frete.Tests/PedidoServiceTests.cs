using Frete.Application.DTOs;
using Frete.Application.Services;
using Frete.Domain.Entities;
using Frete.Domain.Enums;
using Frete.Domain.Interfaces;
using Frete.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Frete.Tests;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _mockRepository;
    private readonly Mock<IFreteStrategyResolver> _mockResolver;
    private readonly Mock<IFreteStrategy> _mockStrategy;
    private readonly PedidoService _pedidoService;

    public PedidoServiceTests()
    {
        _mockRepository = new Mock<IPedidoRepository>();
        _mockResolver = new Mock<IFreteStrategyResolver>();
        _mockStrategy = new Mock<IFreteStrategy>();
        _pedidoService = new PedidoService(_mockRepository.Object, _mockResolver.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCalculateFreteAndCreatePedido()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new PedidoCreateRequest(clientId, ModalidadeFrete.Normal, 5m, 10m, 2m);
        var expectedFrete = 5.5m;

        _mockResolver.Setup(r => r.Resolve(ModalidadeFrete.Normal)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalcularFrete(It.IsAny<FreteParametros>())).Returns(expectedFrete);

        // Act
        var result = await _pedidoService.CreateAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(ModalidadeFrete.Normal, result.Modalidade);
        Assert.Equal(expectedFrete, result.ValorFrete);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Pedido>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingPedido()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var existingPedido = new Pedido(pedidoId, clientId, 100m, ModalidadeFrete.Normal);
        var request = new PedidoUpdateRequest(pedidoId, ModalidadeFrete.Normal, 5m, 10m, 2m);
        var expectedFrete = 5.5m;

        _mockRepository.Setup(r => r.GetByIdAsync(pedidoId, default)).ReturnsAsync(existingPedido);
        _mockResolver.Setup(r => r.Resolve(ModalidadeFrete.Normal)).Returns(_mockStrategy.Object);
        _mockStrategy.Setup(s => s.CalcularFrete(It.IsAny<FreteParametros>())).Returns(expectedFrete);

        // Act
        var result = await _pedidoService.UpdateAsync(request);

        // Assert
        Assert.Equal(pedidoId, result.Id);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(ModalidadeFrete.Normal, result.Modalidade);
        Assert.Equal(expectedFrete, result.ValorFrete);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Pedido>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPedido_WhenExists()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var pedido = new Pedido(pedidoId, clientId, 20m, ModalidadeFrete.Normal);

        _mockRepository.Setup(r => r.GetByIdAsync(pedidoId, CancellationToken.None)).ReturnsAsync(pedido);

        // Act
        var result = await _pedidoService.GetByIdAsync(pedidoId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pedidoId, result.Id);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(20m, result.ValorFrete);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(pedidoId, CancellationToken.None)).ReturnsAsync((Pedido?)null);

        // Act
        var result = await _pedidoService.GetByIdAsync(pedidoId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPedidos()
    {
        // Arrange
        var pedidos = new List<Pedido>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), 10m, ModalidadeFrete.Normal),
            new(Guid.NewGuid(), Guid.NewGuid(), 20m, ModalidadeFrete.Expressa),
            new(Guid.NewGuid(), Guid.NewGuid(), 15m, ModalidadeFrete.Agendada)
        };

        _mockRepository.Setup(r => r.GetAllAsync(CancellationToken.None)).ReturnsAsync(pedidos);

        // Act
        var result = await _pedidoService.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Contains(result, p => p.ValorFrete == 10m);
        Assert.Contains(result, p => p.ValorFrete == 20m);
        Assert.Contains(result, p => p.ValorFrete == 15m);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPedidos()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(CancellationToken.None)).ReturnsAsync(new List<Pedido>());

        // Act
        var result = await _pedidoService.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();

        // Act
        await _pedidoService.DeleteAsync(pedidoId);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(pedidoId, CancellationToken.None), Times.Once);
    }
}