using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthApplicationService authService) : ApiControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var result = authService.Register(request.name, request.email, request.password);
        return FromResult(result, payload =>
            Created($"/api/users/{payload.User.Id}", new { token = payload.Token, user = payload.User.ToResponse() }));
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = authService.Login(request.email, request.password);
        return FromResult(result, payload => Ok(new { token = payload.Token, user = payload.User.ToResponse() }));
    }
}
