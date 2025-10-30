using System.Net;
using System.Text.Json;
using Frete.Application.DTOs;
using Frete.Domain.Exceptions;

namespace Frete.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ErrorResponse errorResponse;

        switch (exception)
        {
            case PedidoNotFoundException notFoundEx:
                _logger.LogWarning(notFoundEx, "Pedido não encontrado: {PedidoId}", notFoundEx.PedidoId);
                errorResponse = ErrorResponse.NotFound(notFoundEx.Message);
                break;

            case PedidoAlreadyExistsException conflictEx:
                _logger.LogWarning(conflictEx, "Pedido já existe: {PedidoId}", conflictEx.PedidoId);
                errorResponse = ErrorResponse.Conflict(conflictEx.Message);
                break;

            case InvalidFreteParametrosException invalidParamEx:
                _logger.LogWarning(invalidParamEx, "Parâmetros de frete inválidos: {Message}", invalidParamEx.Message);
                errorResponse = ErrorResponse.BadRequest(invalidParamEx.Message);
                break;

            case ArgumentNullException argNullEx:
                _logger.LogWarning(argNullEx, "Argumento nulo: {ParamName}", argNullEx.ParamName);
                errorResponse = ErrorResponse.BadRequest(
                    "Um parâmetro obrigatório não foi fornecido.",
                    argNullEx.ParamName);
                break;

            case ArgumentException argEx:
                _logger.LogWarning(argEx, "Argumento inválido: {ParamName} - {Message}", argEx.ParamName, argEx.Message);
                errorResponse = ErrorResponse.BadRequest(argEx.Message, argEx.ParamName);
                break;

            default:
                _logger.LogError(exception, "Erro não tratado: {ExceptionType} - {Message}",
                    exception.GetType().Name, exception.Message);
                errorResponse = ErrorResponse.InternalError();
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.Status;

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
