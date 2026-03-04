using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Features.Users.Queries;
using Server.Domain.Entities;
using Server.Presentation.Auditing;
using Server.Presentation.Authorization;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/audit-logs")]
public sealed class AuditLogsController(ICqrsDispatcher cqrsDispatcher) : ControllerBase
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    [HttpGet("list-access")]
    [AdminOnly]
    public async Task<IActionResult> GetListAccessAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = DefaultPageSize, CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        var allItems = await GetAllAuditItems(cancellationToken);

        var totalItems = allItems.Count;
        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)safePageSize);
        var normalizedPage = Math.Min(safePage, totalPages);
        var skip = (normalizedPage - 1) * safePageSize;
        var pagedItems = allItems.Skip(skip).Take(safePageSize).ToList();

        return Ok(new
        {
            items = pagedItems,
            pagination = new
            {
                page = normalizedPage,
                pageSize = safePageSize,
                totalItems,
                totalPages,
            },
        });
    }

    private async Task<List<AuditLogListItem>> GetAllAuditItems(CancellationToken cancellationToken)
    {
        var claimLogs = await cqrsDispatcher.ExecuteQuery<GetAllClaimsAuditLogsQuery, IEnumerable<ClaimAuditLogEntry>>(new GetAllClaimsAuditLogsQuery(), cancellationToken);
        var userLogs = await cqrsDispatcher.ExecuteQuery<GetAllUsersAuditLogsQuery, IEnumerable<AuditLogEntry>>(new GetAllUsersAuditLogsQuery(), cancellationToken);
        var insuranceLogs = await cqrsDispatcher.ExecuteQuery<GetAllHealthInsuranceAuditLogsQuery, IEnumerable<AuditLogEntry>>(new GetAllHealthInsuranceAuditLogsQuery(), cancellationToken);

        return claimLogs
            .Select(item => new AuditLogListItem(item.Id, item.ClaimId, item.Action, item.Field, item.OldValue, item.NewValue, item.PerformedBy, item.OccurredAtUtc))
            .Concat(userLogs.Select(item => new AuditLogListItem(item.Id, item.EntityId, item.Action, item.Field, item.OldValue, item.NewValue, item.PerformedBy, item.OccurredAtUtc)))
            .Concat(insuranceLogs.Select(item => new AuditLogListItem(item.Id, item.EntityId, item.Action, item.Field, item.OldValue, item.NewValue, item.PerformedBy, item.OccurredAtUtc)))
            .OrderByDescending(item => item.OccurredAtUtc)
            .ToList();
    }

    private sealed record AuditLogListItem(
        string Id,
        string EntityId,
        string Action,
        string Field,
        string? OldValue,
        string? NewValue,
        string PerformedBy,
        DateTime OccurredAtUtc);
}
