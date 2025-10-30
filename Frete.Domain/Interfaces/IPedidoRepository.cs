using Frete.Domain.Entities;

namespace Frete.Domain.Interfaces;

public interface IPedidoRepository
{
    Task<Pedido?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Pedido pedido, CancellationToken ct = default);
    Task UpdateAsync(Pedido pedido, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}