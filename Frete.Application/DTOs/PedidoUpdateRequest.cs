using Frete.Domain.Enums;

namespace Frete.Application.DTOs;

public record PedidoUpdateRequest(ModalidadeFrete Modalidade, decimal PesoKg, decimal DistanciaKm, decimal TaxaFixa);