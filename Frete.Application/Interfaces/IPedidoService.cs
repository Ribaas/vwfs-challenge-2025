using Frete.Application.DTOs;

namespace Frete.Application.Interfaces;

public interface IPedidoService
{
    Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, CancellationToken ct = default);
    Task<PedidoResponse> UpdateAsync(PedidoUpdateRequest req, CancellationToken ct = default);
    Task<PedidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<PedidoResponse>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}