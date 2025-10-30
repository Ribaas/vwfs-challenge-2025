using Frete.Application.Strategies;
using Frete.Domain.Enums;
using Frete.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Frete.Application.Services;

public class FreteStrategyResolver : IFreteStrategyResolver
{
    private readonly IServiceProvider _serviceProvider;

    public FreteStrategyResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFreteStrategy Resolve(ModalidadeFrete modalidade)
    {
        switch (modalidade)
        {
            case ModalidadeFrete.Normal:
                return _serviceProvider.GetRequiredService<NormalFreteStrategy>();
            case ModalidadeFrete.Expressa:
                return _serviceProvider.GetRequiredService<ExpressaFreteStrategy>();
            case ModalidadeFrete.Agendada:
                return _serviceProvider.GetRequiredService<AgendadaFreteStrategy>();
            default:
                throw new ArgumentException($"Modalidade de frete invalida: {modalidade}");
        }
    }
}