using Prensadao.API.ErrorHandling;

namespace Prensadao.API.Extensions;

public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddApiErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        // No .NET 8, o middleware também registra exceções tratadas pelo handler.
        services.AddLogging(logging => logging.AddFilter(
            "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None));
        return services;
    }
}
