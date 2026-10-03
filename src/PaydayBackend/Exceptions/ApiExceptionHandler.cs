using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PaydayBackend.Exceptions;

/// <summary>
/// Maps the domain exceptions thrown by repositories/services to meaningful HTTP
/// status codes instead of letting them bubble up as generic 500s.
/// </summary>
public class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (statusCode, title) = exception switch
        {
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Entity not found"),
            DuplicateEntityException => (StatusCodes.Status409Conflict, "Duplicate entity"),
            InvalidContractStateException => (
                StatusCodes.Status409Conflict,
                "Invalid contract state"
            ),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
            ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            _ => (0, string.Empty),
        };

        if (statusCode == 0)
            return false;

        logger.LogInformation(
            exception,
            "Handled {ExceptionType} as a {StatusCode} response",
            exception.GetType().Name,
            statusCode
        );

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
            },
            cancellationToken
        );

        return true;
    }
}
