using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Auditing;
using Server.Presentation.Authorization;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUsersApplicationService usersService) : ApiControllerBase
{
    private static readonly TimeSpan ReadAuditThrottle = TimeSpan.FromMinutes(2);
    private const int ListAuditMaxItems = 100;
    private const string AuditScope = "users";
    private const string ListAuditEntityId = "_LIST_";

    [HttpGet]
    [AdminOnly]
    public IActionResult List()
    {
        AddReadAuditLog(GetActor());
        return Ok(new { items = usersService.List().Select(user => user.ToResponse()) });
    }

    [HttpGet("audit-logs/list-access")]
    [AdminOnly]
    public IActionResult GetListAccessAuditLogs()
    {
        var items = AuditLogStore
            .Query(AuditScope, ListAuditEntityId, ListAuditMaxItems)
            .Select(entry => new UserAuditLogResponse(
                entry.Id,
                entry.Action,
                entry.Field,
                entry.OldValue,
                entry.NewValue,
                entry.PerformedBy,
                entry.OccurredAtUtc))
            .ToList();

        return Ok(new { items });
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

    private void AddReadAuditLog(string actor)
    {
        AuditLogStore.AddReadWithThrottle(AuditScope, ListAuditEntityId, "UserList", actor, ReadAuditThrottle);
    }

    private string GetActor()
    {
        var userName = User.FindFirstValue(JwtRegisteredClaimNames.Name);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(email))
        {
            return $"{userName} ({email})";
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            return userName;
        }

        return subject ?? "system";
    }
}
