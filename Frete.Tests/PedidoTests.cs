using Frete.Domain.Entities;
using Frete.Domain.Enums;

namespace Frete.Tests;

public class PedidoTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var valorFrete = 100m;
        var modalidade = ModalidadeFrete.Expressa;

        // Act
        var pedido = new Pedido(id, clientId, valorFrete, modalidade);

        // Assert
        Assert.Equal(id, pedido.Id);
        Assert.Equal(clientId, pedido.ClientId);
        Assert.Equal(valorFrete, pedido.ValorFrete);
        Assert.Equal(modalidade, pedido.Modalidade);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenIdIsGuidEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Pedido(Guid.Empty, Guid.NewGuid(), 100m, ModalidadeFrete.Normal));
        Assert.Equal("O ID do pedido não pode ser vazio. (Parameter 'id')", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenClientIdIsGuidEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Pedido(Guid.NewGuid(), Guid.Empty, 100m, ModalidadeFrete.Normal));
        Assert.Equal("O ID do cliente não pode ser vazio. (Parameter 'clientId')", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenValorFreteIsNegative()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Pedido(Guid.NewGuid(), Guid.NewGuid(), -10m, ModalidadeFrete.Normal));
        Assert.Equal("O valor do frete não pode ser negativo. (Parameter 'valorFrete')", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldPreserveGuid_WhenIdIsProvided()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var pedido = new Pedido(id, Guid.NewGuid(), 100m, ModalidadeFrete.Normal);

        // Assert
        Assert.Equal(id, pedido.Id);
    }

    [Theory]
    [InlineData(50.0)]
    [InlineData(100.5)]
    [InlineData(0.01)]
    [InlineData(0)]
    public void ComValorFrete_ShouldCreateNewPedidoWithNewValue_WhenValueIsValid(decimal novoValor)
    {
        // Arrange
        var pedidoOriginal = new Pedido(Guid.NewGuid(), Guid.NewGuid(), 100m, ModalidadeFrete.Normal);

        // Act
        var pedidoAtualizado = pedidoOriginal.ComValorFrete(novoValor);

        // Assert
        Assert.Equal(novoValor, pedidoAtualizado.ValorFrete);
        Assert.Equal(pedidoOriginal.Id, pedidoAtualizado.Id);
        Assert.Equal(pedidoOriginal.ClientId, pedidoAtualizado.ClientId);
        Assert.Equal(pedidoOriginal.Modalidade, pedidoAtualizado.Modalidade);
        Assert.NotSame(pedidoOriginal, pedidoAtualizado);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100.5)]
    public void ComValorFrete_ShouldThrowArgumentException_WhenValueIsNegative(decimal valorInvalido)
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid(), Guid.NewGuid(), 100m, ModalidadeFrete.Normal);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => pedido.ComValorFrete(valorInvalido));
        Assert.Equal("Valor de frete não pode ser negativo. (Parameter 'novoValor')", exception.Message);
    }

    [Fact]
    public void ComModalidade_ShouldCreateNewOrderWithNewModality()
    {
        // Arrange
        var pedidoOriginal = new Pedido(Guid.NewGuid(), Guid.NewGuid(), 100m, ModalidadeFrete.Normal);
        var novaModalidade = ModalidadeFrete.Expressa;

        // Act
        var pedidoAtualizado = pedidoOriginal.ComModalidade(novaModalidade);

        // Assert
        Assert.Equal(novaModalidade, pedidoAtualizado.Modalidade);
        Assert.Equal(pedidoOriginal.Id, pedidoAtualizado.Id);
        Assert.Equal(pedidoOriginal.ClientId, pedidoAtualizado.ClientId);
        Assert.Equal(pedidoOriginal.ValorFrete, pedidoAtualizado.ValorFrete);
        Assert.NotSame(pedidoOriginal, pedidoAtualizado);
    }
}