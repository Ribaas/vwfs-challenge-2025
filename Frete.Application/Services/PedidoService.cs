using Frete.Application.DTOs;
using Frete.Application.Interfaces;
using Frete.Domain.Entities;
using Frete.Domain.Enums;
using Frete.Domain.Interfaces;
using Frete.Domain.ValueObjects;

namespace Frete.Application.Services;

public sealed class PedidoService (
    IPedidoRepository repository,
    IFreteStrategyResolver resolver) : IPedidoService
{
    
    public async Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, CancellationToken ct = default)
    {
        var valor = CalcularFrete(req.Modalidade, new(req.PesoKg, req.DistanciaKm, req.TaxaFixa));
        var pedido = new Pedido(Guid.NewGuid(), req.ClientId, valor, req.Modalidade);
        await repository.AddAsync(pedido, ct);
        return Map(pedido);
    }

    public async Task<PedidoResponse> UpdateAsync(PedidoUpdateRequest req, CancellationToken ct = default)
    {
        var existente = await repository.GetByIdAsync(req.Id, ct) ?? throw new KeyNotFoundException("Pedido não encontrado.");
        var valor = CalcularFrete(req.Modalidade, new(req.PesoKg, req.DistanciaKm, req.TaxaFixa));
        var atualizado = existente.ComValorFrete(valor).ComModalidade(req.Modalidade);
        await repository.UpdateAsync(atualizado, ct);
        return Map(atualizado);
    }

    public async Task<PedidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        (await repository.GetByIdAsync(id, ct)) is { } p ? Map(p) : null;

    public async Task<IEnumerable<PedidoResponse>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(Map).ToList();

    public Task DeleteAsync(Guid id, CancellationToken ct = default) => repository.DeleteAsync(id, ct);

    private static PedidoResponse Map(Pedido p) => new(p.Id, p.ClientId, p.Modalidade, p.ValorFrete);

    private decimal CalcularFrete(ModalidadeFrete modalidade, FreteParametros p)
    {
        var strategy = resolver.Resolve(modalidade);
        return strategy.CalcularFrete(p);
    }
    
}