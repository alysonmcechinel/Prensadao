using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prensadao.Application.Exceptions;
using System.Diagnostics;

namespace Prensadao.Infra;

public class RequestContextMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestContextMiddleware> _logger;

    public RequestContextMiddleware(RequestDelegate next, ILogger<RequestContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items[CorrelationIdHeader] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (AiResponseValidationException ex)
        {
            _logger.LogWarning(ex, "AI validation failure. CorrelationId: {CorrelationId}", correlationId);
            await WriteProblemDetailsAsync(context, StatusCodes.Status422UnprocessableEntity, "AI_RESPONSE_INVALID", ex.Message, correlationId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure AI request failure. CorrelationId: {CorrelationId}", correlationId);
            await WriteProblemDetailsAsync(context, StatusCodes.Status502BadGateway, "AI_PROVIDER_ERROR", "Falha ao obter sugestão de promoção.", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "UNEXPECTED_ERROR", "Erro inesperado no processamento da requisição.", correlationId);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("{Method} {Path} finalizado em {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string code, string detail, string correlationId)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = "Request failed",
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problem.Extensions["code"] = code;
        problem.Extensions["correlationId"] = correlationId;

        await context.Response.WriteAsJsonAsync(problem);
    }
}
