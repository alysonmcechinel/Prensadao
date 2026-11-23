using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.Exceptions;

namespace Prensadao.API.ErrorHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
            return false;

        var (status, title, detail) = exception switch
        {
            RequestValidationException => (400, "Request inválida", exception.Message),
            ResourceNotFoundException => (404, "Recurso não encontrado", exception.Message),
            ConflictException => (409, "Conflito com o estado atual", exception.Message),
            AiResponseValidationException => (422, "Resposta inválida da IA", exception.Message),
            InsufficientSalesDataException => (422, "Dados de vendas insuficientes", exception.Message),
            _ => (500, "Erro inesperado", "Ocorreu um erro inesperado ao processar a requisição.")
        };

        var traceId = httpContext.TraceIdentifier;
        logger.Log(status >= 500 ? LogLevel.Error : LogLevel.Warning, exception,
            "HTTP {Method} {Path} failed with {StatusCode}. TraceId={TraceId}",
            httpContext.Request.Method, httpContext.Request.Path.Value, status, traceId);

        httpContext.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path.Value
        };
        problem.Extensions["traceId"] = traceId;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null,
            contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
