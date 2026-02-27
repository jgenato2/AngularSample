using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUsersApplicationService usersService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult List()
    {
        return Ok(new { items = usersService.List().Select(user => user.ToResponse()) });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        var result = usersService.Create(request.name, request.email, request.role, request.password);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ErrorType.Validation => BadRequest(new { message = result.Error }),
                ErrorType.Conflict => Conflict(new { message = result.Error }),
                _ => StatusCode(500, new { message = "Unexpected error." }),
            };
        }

        var createdUser = result.Value!;
        return Created($"/api/users/{createdUser.Id}", new { item = createdUser.ToResponse() });
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "SelfOrAdmin")]
    public IActionResult GetById(string id)
    {
        var result = usersService.GetById(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.Error });
        }

        var user = result.Value!;
        return Ok(new { item = user.ToResponse() });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SelfOrAdmin")]
    public IActionResult Update(string id, [FromBody] UpdateUserRequest request)
    {
        var updates = new UpdateUserModel
        {
            Name = request.name,
            Email = request.email,
            Role = request.role,
            Password = request.password,
        };

        var result = usersService.Update(id, updates, User.IsInRole("admin"));
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(new { message = result.Error }),
                ErrorType.Conflict => Conflict(new { message = result.Error }),
                _ => StatusCode(500, new { message = "Unexpected error." }),
            };
        }

        return Ok(new { item = result.Value!.ToResponse() });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Delete(string id)
    {
        var result = usersService.Delete(id);
        if (!result.Success)
        {
            return NotFound(new { message = result.Error });
        }

        return Ok(new { ok = true });
    }
}
