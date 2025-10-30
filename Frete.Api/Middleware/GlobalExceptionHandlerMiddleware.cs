using System.Net;
using System.Text.Json;
using Frete.Application.DTOs;
using Frete.Domain.Exceptions;

namespace Frete.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next)
    {
        _next = next;
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
                errorResponse = ErrorResponse.NotFound(notFoundEx.Message);
                break;

            case PedidoAlreadyExistsException conflictEx:
                errorResponse = ErrorResponse.Conflict(conflictEx.Message);
                break;

            case InvalidFreteParametrosException invalidParamEx:
                errorResponse = ErrorResponse.BadRequest(invalidParamEx.Message);
                break;
            
            case ArgumentNullException argNullEx:
                errorResponse = ErrorResponse.BadRequest(
                    "Um parâmetro obrigatório não foi fornecido.",
                    argNullEx.ParamName);
                break;

            case ArgumentException argEx:
                errorResponse = ErrorResponse.BadRequest(argEx.Message, argEx.ParamName);
                break;

            default:
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
