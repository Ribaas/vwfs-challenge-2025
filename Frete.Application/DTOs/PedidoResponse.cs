using Frete.Domain.Enums;

namespace Frete.Application.DTOs;

public record PedidoResponse(Guid Id, Guid ClientId, ModalidadeFrete Modalidade, decimal ValorFrete);