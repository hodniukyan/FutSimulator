using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniBrasfoot.Application.Validators;
using MySqlConnector;

namespace MiniBrasfoot.Api.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            UseCaseException { Kind: FailureKind.Validation } ex => (400, ex.Message),
            UseCaseException { Kind: FailureKind.NotFound } ex => (404, ex.Message),
            UseCaseException { Kind: FailureKind.Conflict } ex => (409, ex.Message),
            BadHttpRequestException => (400, "Requisição inválida."),
            DbUpdateException { InnerException: MySqlException { Number: 1451 or 1452 } } =>
                (409, "A operação conflita com o relacionamento entre time e jogadores."),
            MySqlException => (503, "Banco de dados indisponível."),
            DbUpdateException => (503, "Não foi possível salvar os dados."),
            _ => (500, "Ocorreu um erro inesperado.")
        };
        if (status >= 500)
            logger.LogError("Falha {ExceptionType} na requisição {TraceId} {Method} {Path}",
                exception.GetType().Name, context.TraceIdentifier, context.Request.Method, context.Request.Path);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = title, Instance = context.Request.Path,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
