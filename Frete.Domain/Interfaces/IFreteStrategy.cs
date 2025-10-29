using Frete.Domain.ValueObjects;

namespace Frete.Domain.Interfaces;

public interface IFreteStrategy
{
    decimal CalcularFrete(FreteParametros parametros);
}