using Microsoft.AspNetCore.Mvc;
using ReviewPortal.Application.Common;

namespace ReviewPortal.API.Extensions;

public static class ControllerBaseExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value!);
        }

        return result.FailureType switch
        {
            ErrorType.NotFound => controller.Problem(
                title: "Resource not found",
                detail: result.Error,
                statusCode: StatusCodes.Status404NotFound),
            ErrorType.Conflict => controller.Problem(
                title: "Conflict",
                detail: result.Error,
                statusCode: StatusCodes.Status409Conflict),
            ErrorType.Unauthorized => controller.Problem(
                title: "Unauthorized",
                detail: result.Error,
                statusCode: StatusCodes.Status401Unauthorized),
            ErrorType.Forbidden => controller.Problem(
                title: "Forbidden",
                detail: result.Error,
                statusCode: StatusCodes.Status403Forbidden),
            _ => controller.Problem(
                title: "Validation failed",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest)
        };
    }
}
