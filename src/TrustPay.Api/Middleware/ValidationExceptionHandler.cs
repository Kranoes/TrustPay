using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
namespace TrustPay.Api;

    public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FluentValidation.ValidationException validationException)
        {
            return false;
        }
        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
            g => g.Key,
            g => g.Select(e => e.ErrorMessage).ToArray());
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred."
        };
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails,cancellationToken);
        return true;
    }

    }

