using Frete.Application.Interfaces;
using Frete.Application.Services;
using Frete.Application.Strategies;
using Frete.Domain.Interfaces;
using Frete.Infra.Repositories;

namespace Frete.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        
        // DI
        builder.Services.AddSingleton<IPedidoRepository, InMemoryPedidoRepository>();
        builder.Services.AddScoped<NormalFreteStrategy>();
        builder.Services.AddScoped<ExpressaFreteStrategy>();
        builder.Services.AddScoped<AgendadaFreteStrategy>();
        builder.Services.AddScoped<IFreteStrategyResolver, FreteStrategyResolver>();
        builder.Services.AddScoped<IPedidoService, PedidoService>();
        
        
        // Controllers
        builder.Services.AddControllers();

        var app = builder.Build();

        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }
}