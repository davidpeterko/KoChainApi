using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NBitcoin.RPC;
using System.Net;

namespace KoChain.Api.Middleware;

/// <summary>
/// Global exception handler that catches all unhandled exceptions and maps them
/// to structured <see cref="ProblemDetails"/> responses. This ensures the API
/// always returns a consistent JSON error shape regardless of where the exception
/// originates — upstream HTTP calls, the RPC node, or internal logic.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title, detail) = exception switch
        {
            // Blockstream returned 404 — the resource doesn't exist on-chain.
            HttpRequestException { StatusCode: HttpStatusCode.NotFound } ex =>
                (StatusCodes.Status404NotFound,
                 "Resource not found.",
                 ex.Message),

            // Blockstream or another upstream dependency returned a non-success response.
            HttpRequestException ex =>
                (StatusCodes.Status502BadGateway,
                 "Upstream API error.",
                 ex.Message),

            // The Bitcoin RPC node returned an error (e.g. block/tx not found on this node).
            RPCException ex =>
                (StatusCodes.Status502BadGateway,
                 "Bitcoin node error.",
                 ex.Message),

            // Service layer signalled that a resource could not be found.
            InvalidOperationException ex =>
                (StatusCodes.Status404NotFound,
                 "Resource not found.",
                 ex.Message),

            // Anything else is an unexpected server error.
            _ =>
                (StatusCodes.Status500InternalServerError,
                 "An unexpected error occurred.",
                 exception.Message)
        };

        _logger.LogError(exception, "Unhandled exception: {Title}", title);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);

        return true;
    }
}
