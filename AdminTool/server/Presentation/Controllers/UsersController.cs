using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Features.Users.Commands;
using Server.Application.Features.Users.Queries;
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
public class UsersController(ICqrsDispatcher cqrsDispatcher) : ApiControllerBase
{
    [HttpGet]
    [AdminOnly]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var query = new ListUsersQuery(GetActor());
        var items = await cqrsDispatcher.ExecuteQuery<ListUsersQuery, IEnumerable<Domain.Entities.User>>(query, cancellationToken);
        return Ok(new { items = items.Select(user => user.ToResponse()) });
    }

    [HttpGet("audit-logs/list-access")]
    [AdminOnly]
    public async Task<IActionResult> GetListAccessAuditLogs(CancellationToken cancellationToken)
    {
        var query = new GetUsersListAccessAuditLogsQuery();
        var items = (await cqrsDispatcher.ExecuteQuery<GetUsersListAccessAuditLogsQuery, IEnumerable<AuditLogEntry>>(query, cancellationToken))
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
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.name, request.email, request.role, request.password, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<CreateUserCommand, OperationResult<Domain.Entities.User>>(command, cancellationToken);
        return FromResult(result, createdUser =>
            Created($"/api/users/{createdUser.Id}", new { item = createdUser.ToResponse() }));
    }

    [HttpGet("{id}")]
    [SelfOrAdmin]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await cqrsDispatcher.ExecuteQuery<GetUserByIdQuery, OperationResult<Domain.Entities.User>>(query, cancellationToken);
        return FromResult(result, user => Ok(new { item = user.ToResponse() }));
    }

    [HttpGet("{id}/audit-logs")]
    [SelfOrAdmin]
    public async Task<IActionResult> GetAuditLogs(string id, CancellationToken cancellationToken)
    {
        var query = new GetUserAuditLogsQuery(id);
        var result = await cqrsDispatcher.ExecuteQuery<GetUserAuditLogsQuery, OperationResult<IEnumerable<AuditLogEntry>>>(query, cancellationToken);
        return FromResult(result, items => Ok(new
        {
            items = items.Select(entry => new UserAuditLogResponse(
                entry.Id,
                entry.Action,
                entry.Field,
                entry.OldValue,
                entry.NewValue,
                entry.PerformedBy,
                entry.OccurredAtUtc)),
        }));
    }

    [HttpPut("{id}")]
    [SelfOrAdmin]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var updates = new UpdateUserModel
        {
            Name = request.name,
            Email = request.email,
            Role = request.role,
            Password = request.password,
        };

        var command = new UpdateUserCommand(id, updates, User.IsInRole("admin"), GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<UpdateUserCommand, OperationResult<Domain.Entities.User>>(command, cancellationToken);
        return FromResult(result, user => Ok(new { item = user.ToResponse() }));
    }

    [HttpDelete("{id}")]
    [AdminOnly]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<DeleteUserCommand, OperationResult<bool>>(command, cancellationToken);
        return FromResult(result, _ => Ok(new { ok = true }));
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
