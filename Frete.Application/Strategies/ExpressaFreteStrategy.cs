using Frete.Domain.Interfaces;
using Frete.Domain.ValueObjects;

namespace Frete.Application.Strategies;

public class ExpressaFreteStrategy : IFreteStrategy
{
    public decimal CalcularFrete(FreteParametros parametros)
    {
        return parametros.PesoKg * 0.5m + parametros.DistanciaKm * 1m + parametros.TaxaFixa;
    }
}