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
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ClientId = clientId;
        ValorFrete = valorFrete;
        Modalidade = modalidade;
    }
    
    public Pedido ComValorFrete(decimal novoValor) =>
        this with { ValorFrete = novoValor > 0 ? novoValor : throw new ArgumentException("Valor de frete deve ser positivo") };
    
    public Pedido ComModalidade(ModalidadeFrete novaModalidade) =>
        this with { Modalidade = novaModalidade };
}