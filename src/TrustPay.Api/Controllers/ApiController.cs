namespace TrustPay.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TrustPay.Domain.Common;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiController : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return HandleFailure(result.Error);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is not null ? Ok(result.Value) : NoContent();
        }

        return HandleFailure(result.Error);
    }

    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object routeValues)
    {
        if (result.IsSuccess)
        {
            return CreatedAtAction(actionName, routeValues, result.Value);
        }

        return HandleFailure(result.Error);
    }

    protected IActionResult HandleFailure(Error error)
    {
        if (error == Error.None)
        {
            throw new InvalidOperationException("Нельзя обработать отсутствие ошибки как ошибку.");
        }

        return error.Type switch
        {
            ErrorType.NotFound => NotFound(CreateProblemDetails("Not Found", StatusCodes.Status404NotFound, error)),
            ErrorType.Validation => BadRequest(CreateProblemDetails("Validation Error", StatusCodes.Status400BadRequest, error)),
            ErrorType.Conflict => Conflict(CreateProblemDetails("Conflict", StatusCodes.Status409Conflict, error)),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails("Forbidden", StatusCodes.Status403Forbidden, error)),
            ErrorType.Unauthorized => Unauthorized(CreateProblemDetails("Unauthorized", StatusCodes.Status401Unauthorized, error)),
            _ => BadRequest(CreateProblemDetails("Bad Request", StatusCodes.Status400BadRequest, error))
        };
    }

    private ProblemDetails CreateProblemDetails(string title, int status, Error error)
    {
        return new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = error.Description,
            Extensions =
            {
                { "code", error.Code }
            }
        };
    }
}