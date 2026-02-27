using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Authorization;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUsersApplicationService usersService) : ApiControllerBase
{
    [HttpGet]
    [AdminOnly]
    public IActionResult List()
    {
        return Ok(new { items = usersService.List().Select(user => user.ToResponse()) });
    }

    [HttpPost]
    [AdminOnly]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        var result = usersService.Create(request.name, request.email, request.role, request.password);
        return FromResult(result, createdUser =>
            Created($"/api/users/{createdUser.Id}", new { item = createdUser.ToResponse() }));
    }

    [HttpGet("{id}")]
    [SelfOrAdmin]
    public IActionResult GetById(string id)
    {
        var result = usersService.GetById(id);
        return FromResult(result, user => Ok(new { item = user.ToResponse() }));
    }

    [HttpPut("{id}")]
    [SelfOrAdmin]
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
        return FromResult(result, user => Ok(new { item = user.ToResponse() }));
    }

    [HttpDelete("{id}")]
    [AdminOnly]
    public IActionResult Delete(string id)
    {
        var result = usersService.Delete(id);
        return FromResult(result, _ => Ok(new { ok = true }));
    }
}
