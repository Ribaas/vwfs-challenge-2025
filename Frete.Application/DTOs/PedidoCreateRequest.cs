using Frete.Domain.Enums;

namespace Frete.Application.DTOs;

public record PedidoCreateRequest(Guid ClientId, ModalidadeFrete Modalidade, decimal PesoKg, decimal DistanciaKm, decimal TaxaFixa);