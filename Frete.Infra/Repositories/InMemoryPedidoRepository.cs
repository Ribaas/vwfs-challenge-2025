using System.Collections.Concurrent;
using Frete.Domain.Entities;
using Frete.Domain.Exceptions;
using Frete.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Frete.Infra.Repositories;

public class InMemoryPedidoRepository : IPedidoRepository
{
    private readonly ConcurrentDictionary<Guid, Pedido> _db = new();
    private readonly ILogger<InMemoryPedidoRepository> _logger;

    public InMemoryPedidoRepository(ILogger<InMemoryPedidoRepository> logger)
    {
        _logger = logger;
    }

    public Task AddAsync(Pedido pedido, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        _logger.LogDebug("Tentando adicionar pedido {PedidoId} ao repositório", pedido.Id);

        if (!_db.TryAdd(pedido.Id, pedido))
        {
            _logger.LogWarning("Pedido {PedidoId} já existe no repositório", pedido.Id);
            throw new PedidoAlreadyExistsException(pedido.Id);
        }

        _logger.LogInformation("Pedido {PedidoId} adicionado ao repositório com sucesso", pedido.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Tentando remover pedido {PedidoId} do repositório", id);

        if (!_db.TryRemove(id, out _))
        {
            _logger.LogWarning("Pedido {PedidoId} não encontrado para remoção", id);
            throw new PedidoNotFoundException(id);
        }

        _logger.LogInformation("Pedido {PedidoId} removido do repositório com sucesso", id);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Pedido>> GetAllAsync(CancellationToken ct = default)
    {
        var count = _db.Count;
        _logger.LogDebug("Retornando {Count} pedido(s) do repositório", count);
        return Task.FromResult<IReadOnlyList<Pedido>>(_db.Values.ToList().AsReadOnly());
    }

    public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogDebug("Buscando pedido {PedidoId} no repositório", id);
        _db.TryGetValue(id, out var pedido);

        if (pedido is null)
            _logger.LogDebug("Pedido {PedidoId} não encontrado no repositório", id);
        else
            _logger.LogDebug("Pedido {PedidoId} encontrado no repositório", id);

        return Task.FromResult(pedido);
    }

    public Task UpdateAsync(Pedido pedido, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        _logger.LogDebug("Tentando atualizar pedido {PedidoId} no repositório", pedido.Id);

        if (!_db.ContainsKey(pedido.Id))
        {
            _logger.LogWarning("Pedido {PedidoId} não encontrado para atualização", pedido.Id);
            throw new PedidoNotFoundException(pedido.Id);
        }

        _db[pedido.Id] = pedido;
        _logger.LogInformation("Pedido {PedidoId} atualizado no repositório com sucesso", pedido.Id);
        return Task.CompletedTask;
    }
}