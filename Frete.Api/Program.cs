using Frete.Api.Middleware;
using Frete.Application.Interfaces;
using Frete.Application.Services;
using Frete.Application.Strategies;
using Frete.Domain.Interfaces;
using Frete.Infra.Repositories;

namespace Frete.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Log
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        builder.Logging.AddFilter("Frete", LogLevel.Information);

        builder.Services.AddOpenApi();

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Frete API",
                Version = "v1",
                Description = "API para cálculo de frete de pedidos"
            });
        });

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

        // Log application startup
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Iniciando aplicação Frete API");

        // Global Exception Handler
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.MapControllers();

        logger.LogInformation("Aplicação Frete API configurada e pronta para executar");
        app.Run();
    }
}