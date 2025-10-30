using Frete.Application.DTOs;
using Frete.Application.Interfaces;
using Frete.Domain.Entities;
using Frete.Domain.Enums;
using Frete.Domain.Exceptions;
using Frete.Domain.Interfaces;
using Frete.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Frete.Application.Services;

public sealed class PedidoService (
    IPedidoRepository repository,
    IFreteStrategyResolver resolver,
    ILogger<PedidoService> logger) : IPedidoService
{

    public async Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, CancellationToken ct = default)
    {
        logger.LogInformation("Iniciando criação de pedido para cliente {ClientId} com modalidade {Modalidade}",
            req.ClientId, req.Modalidade);

        if (req.ClientId == Guid.Empty)
        {
            logger.LogWarning("Tentativa de criar pedido com ID de cliente vazio");
            throw new ArgumentException("O ID do cliente não pode ser vazio.", "ClientId");
        }

        var valor = CalcularFrete(req.Modalidade, new(req.PesoKg, req.DistanciaKm, req.TaxaFixa));
        logger.LogDebug("Frete calculado: {ValorFrete} para modalidade {Modalidade}", valor, req.Modalidade);

        var pedido = new Pedido(Guid.NewGuid(), req.ClientId, valor, req.Modalidade);
        await repository.AddAsync(pedido, ct);

        logger.LogInformation("Pedido {PedidoId} criado com sucesso para cliente {ClientId} com valor {ValorFrete}",
            pedido.Id, pedido.ClientId, pedido.ValorFrete);

        return Map(pedido);
    }

    public async Task<PedidoResponse> UpdateAsync(Guid id, PedidoUpdateRequest req, CancellationToken ct = default)
    {
        logger.LogInformation("Iniciando atualização do pedido {PedidoId} com modalidade {Modalidade}",
            id, req.Modalidade);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Tentativa de atualizar pedido com ID vazio");
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));
        }

        var existente = await repository.GetByIdAsync(id, ct);
        if (existente is null)
        {
            logger.LogWarning("Pedido {PedidoId} não encontrado para atualização", id);
            throw new PedidoNotFoundException(id);
        }

        var valor = CalcularFrete(req.Modalidade, new(req.PesoKg, req.DistanciaKm, req.TaxaFixa));
        logger.LogDebug("Novo frete calculado: {ValorFrete} (anterior: {ValorAnterior})", valor, existente.ValorFrete);

        var atualizado = existente.ComValorFrete(valor).ComModalidade(req.Modalidade);
        await repository.UpdateAsync(atualizado, ct);

        logger.LogInformation("Pedido {PedidoId} atualizado com sucesso. Modalidade: {ModalidadeNova}, Valor: {ValorFrete}",
            id, req.Modalidade, valor);

        return Map(atualizado);
    }

    public async Task<PedidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogDebug("Buscando pedido {PedidoId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Tentativa de buscar pedido com ID vazio");
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));
        }

        var pedido = await repository.GetByIdAsync(id, ct);

        if (pedido is null)
        {
            logger.LogInformation("Pedido {PedidoId} não encontrado", id);
            return null;
        }

        logger.LogDebug("Pedido {PedidoId} encontrado", id);
        return Map(pedido);
    }

    public async Task<IEnumerable<PedidoResponse>> GetAllAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Buscando todos os pedidos");
        var pedidos = (await repository.GetAllAsync(ct)).Select(Map).ToList();
        logger.LogInformation("Retornando {Count} pedido(s)", pedidos.Count);
        return pedidos;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogInformation("Iniciando exclusão do pedido {PedidoId}", id);

        if (id == Guid.Empty)
        {
            logger.LogWarning("Tentativa de deletar pedido com ID vazio");
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));
        }

        await repository.DeleteAsync(id, ct);
        logger.LogInformation("Pedido {PedidoId} excluído com sucesso", id);
    }

    private static PedidoResponse Map(Pedido p) => new(p.Id, p.ClientId, p.Modalidade, p.ValorFrete);

    private decimal CalcularFrete(ModalidadeFrete modalidade, FreteParametros p)
    {
        logger.LogDebug("Calculando frete para modalidade {Modalidade} - Peso: {Peso}kg, Distância: {Distancia}km, Taxa: {Taxa}",
            modalidade, p.PesoKg, p.DistanciaKm, p.TaxaFixa);

        var strategy = resolver.Resolve(modalidade);
        var valor = strategy.CalcularFrete(p);

        logger.LogDebug("Frete calculado: {ValorFrete}", valor);
        return valor;
    }

}