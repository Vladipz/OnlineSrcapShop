using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using ProductCatalog.Api.Parsing;

namespace ProductCatalog.Api.Common.ErrorHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled; trace {TraceId}", context.TraceIdentifier);
            context.Abort();
            return true;
        }

        var (status, title, detail) = exception switch
        {
            SourceParseException => (502, "Source parsing failed", "The supported source could not be loaded or recognized."),
            DbUpdateConcurrencyException => (409, "Data conflict", "The product changed during this operation. Reload and retry."),
            DbUpdateException { InnerException: SqliteException { SqliteErrorCode: 19 } } =>
                (409, "Data conflict", "The data conflicts with an existing record. Reload and retry."),
            BadHttpRequestException badRequest => (badRequest.StatusCode, "Invalid request", "The request could not be processed."),
            _ => (500, "Unexpected server error", "An unexpected error occurred.")
        };

        logger.LogError(exception, "Request failed with status {StatusCode}; trace {TraceId}", status, context.TraceIdentifier);
        await Results.Problem(statusCode: status, title: title, detail: detail,
            extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier })
            .ExecuteAsync(context);
        return true;
    }
}
