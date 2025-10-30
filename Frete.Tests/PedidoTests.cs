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
    public void Constructor_ShouldGenerateNewGuid_WhenIdIsGuidEmpty()
    {
        // Arrange & Act
        var pedido = new Pedido(Guid.Empty, Guid.NewGuid(), 100m, ModalidadeFrete.Normal);

        // Assert
        Assert.NotEqual(Guid.Empty, pedido.Id);
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
    public void ComValorFrete_ShouldCreateNewPedidoWithNewValue_WhenValueIsPositive(decimal novoValor)
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
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.5)]
    public void ComValorFrete_ShouldThrowArgumentException_WhenValueIsZeroOrNegative(decimal valorInvalido)
    {
        // Arrange
        var pedido = new Pedido(Guid.NewGuid(), Guid.NewGuid(), 100m, ModalidadeFrete.Normal);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => pedido.ComValorFrete(valorInvalido));
        Assert.Equal("Valor de frete deve ser positivo", exception.Message);
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