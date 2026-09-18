using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DeskFlow.Api.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled DeskFlow exception");
        var (status, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operação inválida"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception is ArgumentException or InvalidOperationException ? exception.Message : "Ocorreu um erro inesperado."
        }, cancellationToken);
        return true;
    }
}
