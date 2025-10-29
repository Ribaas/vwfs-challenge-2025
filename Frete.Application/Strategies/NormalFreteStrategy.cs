using Frete.Domain.Interfaces;
using Frete.Domain.ValueObjects;

namespace Frete.Application.Strategies;

public class NormalFreteStrategy : IFreteStrategy
{
    public decimal CalcularFrete(FreteParametros parametros)
    {
        return parametros.PesoKg * 0.5m + parametros.DistanciaKm * 0.1m + parametros.TaxaFixa;
    }
}