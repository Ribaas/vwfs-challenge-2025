using Frete.Domain.Enums;

namespace Frete.Domain.Interfaces;

public interface IFreteStrategyResolver
{
    IFreteStrategy Resolve(ModalidadeFrete modalidade);
}