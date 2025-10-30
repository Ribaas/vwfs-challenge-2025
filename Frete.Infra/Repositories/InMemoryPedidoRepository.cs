using System.Collections.Concurrent;
using Frete.Domain.Entities;
using Frete.Domain.Exceptions;
using Frete.Domain.Interfaces;

namespace Frete.Infra.Repositories;

public class InMemoryPedidoRepository : IPedidoRepository
{
    private readonly ConcurrentDictionary<Guid, Pedido> _db = new();

    public Task AddAsync(Pedido pedido, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        if (!_db.TryAdd(pedido.Id, pedido))
        {
            throw new PedidoAlreadyExistsException(pedido.Id);
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (!_db.TryRemove(id, out _))
        {
            throw new PedidoNotFoundException(id);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Pedido>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Pedido>>(_db.Values.ToList().AsReadOnly());

    public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _db.TryGetValue(id, out var pedido);
        return Task.FromResult(pedido);
    }

    public Task UpdateAsync(Pedido pedido, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        if (!_db.ContainsKey(pedido.Id))
        {
            throw new PedidoNotFoundException(pedido.Id);
        }

        _db[pedido.Id] = pedido;
        return Task.CompletedTask;
    }
}