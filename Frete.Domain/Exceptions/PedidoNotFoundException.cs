namespace Frete.Domain.Exceptions;

public class PedidoNotFoundException : DomainException
{
    public Guid PedidoId { get; }

    public PedidoNotFoundException(Guid pedidoId)
        : base($"Pedido com ID '{pedidoId}' não foi encontrado.")
    {
        PedidoId = pedidoId;
    }
}
