using Microsoft.AspNetCore.Mvc;
using Server.Application.Models;

namespace Server.Presentation.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(OperationResult<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.Success)
        {
            return onSuccess(result.Value!);
        }

        return result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(new { message = result.Error }),
            ErrorType.Unauthorized => Unauthorized(),
            ErrorType.NotFound => NotFound(new { message = result.Error }),
            ErrorType.Conflict => Conflict(new { message = result.Error }),
            _ => StatusCode(500, new { message = "Unexpected error." }),
        };
    }
}
