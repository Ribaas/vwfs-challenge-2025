namespace Frete.Application.DTOs;

public record ErrorResponse(string Error, int Status, string? Details = null)
{
    public static ErrorResponse NotFound(string message) =>
        new(message, 404);

    public static ErrorResponse BadRequest(string message, string? details = null) =>
        new(message, 400, details);

    public static ErrorResponse Conflict(string message) =>
        new(message, 409);

    public static ErrorResponse InternalError(string message = "Um erro interno ocorreu.") =>
        new(message, 500);
}
