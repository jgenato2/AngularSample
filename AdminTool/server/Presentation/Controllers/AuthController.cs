using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Features.Auth.Commands;
using Server.Application.Models;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ICqrsDispatcher cqrsDispatcher) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.name, request.email, request.password);
        var result = await cqrsDispatcher.ExecuteCommand<RegisterCommand, OperationResult<AuthPayload>>(command, cancellationToken);
        return FromResult(result, payload =>
            Created($"/api/users/{payload.User.Id}", new { token = payload.Token, user = payload.User.ToResponse() }));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.email, request.password);
        var result = await cqrsDispatcher.ExecuteCommand<LoginCommand, OperationResult<AuthPayload>>(command, cancellationToken);
        return FromResult(result, payload => Ok(new { token = payload.Token, user = payload.User.ToResponse() }));
    }
}
