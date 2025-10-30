using Frete.Domain.Enums;

namespace Frete.Domain.Entities;

public sealed record Pedido
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public decimal ValorFrete { get; init; }
    public ModalidadeFrete Modalidade { get; init; }

    public Pedido(Guid id, Guid clientId, decimal valorFrete, ModalidadeFrete modalidade)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O ID do pedido não pode ser vazio.", nameof(id));

        if (clientId == Guid.Empty)
            throw new ArgumentException("O ID do cliente não pode ser vazio.", nameof(clientId));

        if (valorFrete < 0)
            throw new ArgumentException("O valor do frete não pode ser negativo.", nameof(valorFrete));

        if (!Enum.IsDefined(typeof(ModalidadeFrete), modalidade))
            throw new ArgumentException("Modalidade de frete inválida.", nameof(modalidade));

        Id = id;
        ClientId = clientId;
        ValorFrete = valorFrete;
        Modalidade = modalidade;
    }

    public Pedido ComValorFrete(decimal novoValor)
    {
        if (novoValor < 0)
            throw new ArgumentException("Valor de frete não pode ser negativo.", nameof(novoValor));

        return this with { ValorFrete = novoValor };
    }

    public Pedido ComModalidade(ModalidadeFrete novaModalidade)
    {
        if (!Enum.IsDefined(typeof(ModalidadeFrete), novaModalidade))
            throw new ArgumentException("Modalidade de frete inválida.", nameof(novaModalidade));

        return this with { Modalidade = novaModalidade };
    }
}