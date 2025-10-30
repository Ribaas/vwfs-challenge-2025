namespace Frete.Domain.Exceptions;

public class PedidoAlreadyExistsException : DomainException
{
    public Guid PedidoId { get; }

    public PedidoAlreadyExistsException(Guid pedidoId)
        : base($"Pedido com ID '{pedidoId}' já existe.")
    {
        PedidoId = pedidoId;
    }
}
