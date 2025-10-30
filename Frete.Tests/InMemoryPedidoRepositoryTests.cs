using FluentAssertions;
using Frete.Domain.Entities;
using Frete.Domain.Enums;
using Frete.Domain.Exceptions;
using Frete.Infra.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Frete.Tests;

public class InMemoryPedidoRepositoryTests
{
    private readonly InMemoryPedidoRepository _repository;

    public InMemoryPedidoRepositoryTests()
    {
        var mockLogger = new Mock<ILogger<InMemoryPedidoRepository>>();
        _repository = new InMemoryPedidoRepository(mockLogger.Object);
    }

    private static Pedido CreatePedido()
    {
        return new Pedido(Guid.NewGuid(), Guid.NewGuid(), 10m, ModalidadeFrete.Normal);
    }

    [Fact]
    public async Task AddAsync_ShouldAddPedidoSuccessfully()
    {
        // Arrange
        var pedido = CreatePedido();

        // Act
        await _repository.AddAsync(pedido);

        // Assert
        var result = await _repository.GetByIdAsync(pedido.Id);
        result.Should().NotBeNull();
        result.Should().Be(pedido);
    }

    [Fact]
    public async Task AddAsync_WithNullPedido_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _repository.AddAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddAsync_WithDuplicateId_ShouldThrowPedidoAlreadyExistsException()
    {
        // Arrange
        var pedido = CreatePedido();
        await _repository.AddAsync(pedido);

        // Act
        Func<Task> act = async () => await _repository.AddAsync(pedido);

        // Assert
        await act.Should().ThrowAsync<PedidoAlreadyExistsException>()
            .WithMessage($"Pedido com ID '{pedido.Id}' já existe.");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnPedido()
    {
        // Arrange
        var pedido = CreatePedido();
        await _repository.AddAsync(pedido);

        // Act
        var result = await _repository.GetByIdAsync(pedido.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(pedido);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithNoPedidos_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithMultiplePedidos_ShouldReturnAllPedidos()
    {
        // Arrange
        var pedido1 = CreatePedido();
        var pedido2 = CreatePedido();
        await _repository.AddAsync(pedido1);
        await _repository.AddAsync(pedido2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(pedido1);
        result.Should().Contain(pedido2);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingPedido_ShouldUpdateSuccessfully()
    {
        // Arrange
        var pedidoOriginal = CreatePedido();
        await _repository.AddAsync(pedidoOriginal);

        var pedidoAtualizado = pedidoOriginal.ComValorFrete(20m);

        // Act
        await _repository.UpdateAsync(pedidoAtualizado);

        // Assert
        var result = await _repository.GetByIdAsync(pedidoOriginal.Id);
        result.ValorFrete.Should().Be(20m);
        result.Id.Should().Be(pedidoOriginal.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithNullPedido_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingPedido_ShouldThrowPedidoNotFoundException()
    {
        // Arrange
        var pedido = CreatePedido();

        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(pedido);

        // Assert
        await act.Should().ThrowAsync<PedidoNotFoundException>()
            .WithMessage($"Pedido com ID '{pedido.Id}' não foi encontrado.");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldRemovePedido()
    {
        // Arrange
        var pedido = CreatePedido();
        await _repository.AddAsync(pedido);

        // Act
        await _repository.DeleteAsync(pedido.Id);

        // Assert
        var result = await _repository.GetByIdAsync(pedido.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldThrowPedidoNotFoundException()
    {
        // Act
        Func<Task> act = async () => await _repository.DeleteAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<PedidoNotFoundException>();
    }
}