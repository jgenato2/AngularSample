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
    public async Task<IActionResult> List([FromQuery(Name = "sort")] string[]? sort, [FromQuery(Name = "query")] string? queryText, CancellationToken cancellationToken)
    {
        var query = new ListUsersQuery(GetActor());
        var users = await cqrsDispatcher.ExecuteQuery<ListUsersQuery, IEnumerable<Domain.Entities.User>>(query, cancellationToken);
        var filteredUsers = ApplyFiltering(users, queryText);
        var items = ApplySorting(filteredUsers, sort).Select(user => user.ToResponse());
        return Ok(new { items });
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

    private static IEnumerable<Domain.Entities.User> ApplySorting(
        IEnumerable<Domain.Entities.User> items,
        IEnumerable<string>? sortTokens)
    {
        var parsedSorts = ParseSorts(sortTokens).ToList();
        if (parsedSorts.Count == 0)
        {
            return items;
        }

        IOrderedEnumerable<Domain.Entities.User>? ordered = null;
        foreach (var sort in parsedSorts)
        {
            ordered = ApplySort(ordered ?? items, ordered is not null, sort.Field, sort.Descending);
        }

        return ordered ?? items;
    }

    private static IEnumerable<Domain.Entities.User> ApplyFiltering(
        IEnumerable<Domain.Entities.User> users,
        string? queryText)
    {
        var query = queryText?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return users;
        }

        return users.Where(user =>
            user.Id.Contains(query, StringComparison.OrdinalIgnoreCase)
            || user.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || user.Email.Contains(query, StringComparison.OrdinalIgnoreCase)
            || user.Role.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<(string Field, bool Descending)> ParseSorts(IEnumerable<string>? sortTokens)
    {
        foreach (var token in sortTokens ?? Enumerable.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                continue;
            }

            var parts = token.Split(':', 2, StringSplitOptions.TrimEntries);
            var field = parts[0].Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(field))
            {
                continue;
            }

            var descending = parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase);
            yield return (field, descending);
        }
    }

    private static IOrderedEnumerable<Domain.Entities.User> ApplySort(
        IEnumerable<Domain.Entities.User> source,
        bool thenBy,
        string field,
        bool descending)
    {
        return field switch
        {
            "id" => OrderBy(source, thenBy, descending, item => item.Id),
            "name" => OrderBy(source, thenBy, descending, item => item.Name),
            "email" => OrderBy(source, thenBy, descending, item => item.Email),
            "role" => OrderBy(source, thenBy, descending, item => item.Role),
            "createdat" => OrderBy(source, thenBy, descending, item => item.CreatedAt),
            "updatedat" => OrderBy(source, thenBy, descending, item => item.UpdatedAt),
            _ => thenBy ? (IOrderedEnumerable<Domain.Entities.User>)source : source.OrderBy(item => 0),
        };
    }

    private static IOrderedEnumerable<Domain.Entities.User> OrderBy<TKey>(
        IEnumerable<Domain.Entities.User> source,
        bool thenBy,
        bool descending,
        Func<Domain.Entities.User, TKey> keySelector)
    {
        if (thenBy)
        {
            var ordered = (IOrderedEnumerable<Domain.Entities.User>)source;
            return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
        }

        return descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
    }
}
