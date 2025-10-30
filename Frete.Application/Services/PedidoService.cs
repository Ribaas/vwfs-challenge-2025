using Frete.Application.DTOs;
using Frete.Application.Interfaces;
using Frete.Domain.Entities;
using Frete.Domain.Enums;
using Frete.Domain.Exceptions;
using Frete.Domain.Interfaces;
using Frete.Domain.ValueObjects;

namespace Frete.Application.Services;

public sealed class PedidoService (
    IPedidoRepository repository,
    IFreteStrategyResolver resolver) : IPedidoService
{

    public async Task<PedidoResponse> CreateAsync(PedidoCreateRequest req, CancellationToken ct = default)
    {
        if (req.ClientId == Guid.Empty)
            throw new ArgumentException("O ID do cliente não pode ser vazio.", nameof(req.ClientId));

        var valor = CalcularFrete(req.Modalidade, new(req.PesoKg, req.DistanciaKm, req.TaxaFixa));
        var pedido = new Pedido(Guid.NewGuid(), req.ClientId, valor, req.Modalidade);
        await repository.AddAsync(pedido, ct);
        return Map(pedido);
    }

    public async Task<PedidoResponse> UpdateAsync(Guid id, PedidoUpdateRequest req, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));

        var existente = await repository.GetByIdAsync(id, ct)
            ?? throw new PedidoNotFoundException(id);

        var valor = CalcularFrete(req.Modalidade, new(req.PesoKg, req.DistanciaKm, req.TaxaFixa));
        var atualizado = existente.ComValorFrete(valor).ComModalidade(req.Modalidade);
        await repository.UpdateAsync(atualizado, ct);
        return Map(atualizado);
    }

    public async Task<PedidoResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));

        return (await repository.GetByIdAsync(id, ct)) is { } p ? Map(p) : null;
    }

    public async Task<IEnumerable<PedidoResponse>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(Map).ToList();

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));

        await repository.DeleteAsync(id, ct);
    }

    private static PedidoResponse Map(Pedido p) => new(p.Id, p.ClientId, p.Modalidade, p.ValorFrete);

    private decimal CalcularFrete(ModalidadeFrete modalidade, FreteParametros p)
    {
        var strategy = resolver.Resolve(modalidade);
        return strategy.CalcularFrete(p);
    }

}