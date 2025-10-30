using Frete.Application.Strategies;
using Frete.Domain.Enums;
using Frete.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Frete.Application.Services;

public class FreteStrategyResolver : IFreteStrategyResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FreteStrategyResolver> _logger;

    public FreteStrategyResolver(IServiceProvider serviceProvider, ILogger<FreteStrategyResolver> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public IFreteStrategy Resolve(ModalidadeFrete modalidade)
    {
        _logger.LogDebug("Resolvendo estratégia de frete para modalidade {Modalidade}", modalidade);

        switch (modalidade)
        {
            case ModalidadeFrete.Normal:
                _logger.LogDebug("Estratégia Normal selecionada");
                return _serviceProvider.GetRequiredService<NormalFreteStrategy>();
            case ModalidadeFrete.Expressa:
                _logger.LogDebug("Estratégia Expressa selecionada");
                return _serviceProvider.GetRequiredService<ExpressaFreteStrategy>();
            case ModalidadeFrete.Agendada:
                _logger.LogDebug("Estratégia Agendada selecionada");
                return _serviceProvider.GetRequiredService<AgendadaFreteStrategy>();
            default:
                _logger.LogError("Modalidade de frete inválida: {Modalidade}", modalidade);
                throw new ArgumentException($"Modalidade de frete invalida: {modalidade}");
        }
    }
}