using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthApplicationService authService) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var result = authService.Register(request.name, request.email, request.password);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ErrorType.Validation => BadRequest(new { message = result.Error }),
                ErrorType.Conflict => Conflict(new { message = result.Error }),
                _ => StatusCode(500, new { message = "Unexpected error." }),
            };
        }

        var payload = result.Value!;
        return Created($"/api/users/{payload.User.Id}", new { token = payload.Token, user = payload.User.ToResponse() });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = authService.Login(request.email, request.password);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ErrorType.Validation => BadRequest(new { message = result.Error }),
                ErrorType.Unauthorized => Unauthorized(),
                _ => StatusCode(500, new { message = "Unexpected error." }),
            };
        }

        var payload = result.Value!;
        return Ok(new { token = payload.Token, user = payload.User.ToResponse() });
    }
}
