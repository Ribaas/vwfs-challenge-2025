using Frete.Domain.Exceptions;

namespace Frete.Domain.ValueObjects;

public sealed record FreteParametros
{
    public decimal PesoKg { get; init; }
    public decimal DistanciaKm { get; init; }
    public decimal TaxaFixa { get; init; }

    public FreteParametros(decimal pesoKg, decimal distanciaKm, decimal taxaFixa)
    {
        if (pesoKg <= 0)
            throw new InvalidFreteParametrosException("O peso deve ser maior que zero.");

        if (distanciaKm <= 0)
            throw new InvalidFreteParametrosException("A distância deve ser maior que zero.");

        if (taxaFixa < 0)
            throw new InvalidFreteParametrosException("A taxa fixa não pode ser negativa.");

        PesoKg = pesoKg;
        DistanciaKm = distanciaKm;
        TaxaFixa = taxaFixa;
    }
}