using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.BuildingBlocks.Application;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return result.Error!.ToActionResult();
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return result.Error!.ToActionResult();
    }

    private static ObjectResult ToActionResult(this ApplicationError error)
    {
        var status = error.Type switch
        {
            ApplicationErrorType.NotFound => StatusCodes.Status404NotFound,
            ApplicationErrorType.Validation => StatusCodes.Status400BadRequest,
            ApplicationErrorType.Conflict => StatusCodes.Status409Conflict,
            ApplicationErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ApplicationErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ApplicationErrorType.Internal => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Title = error.Code,
            Detail = status == StatusCodes.Status500InternalServerError
            ? "An internal server error occurred."
            : error.Message,
            Status = status
        };

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status
        };
    }
}
