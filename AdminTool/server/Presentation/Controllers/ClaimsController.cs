using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Features.Claims.Commands;
using Server.Application.Features.Claims.Queries;
using Server.Application.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Server.Presentation.Authorization;
using Server.Presentation.Contracts;
using Server.Presentation.Mappings;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/claims")]
public class ClaimsController(ICqrsDispatcher cqrsDispatcher) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery(Name = "sort")] string[]? sort, CancellationToken cancellationToken)
    {
        var query = new ListClaimsQuery(GetActor());
        var claims = await cqrsDispatcher.ExecuteQuery<ListClaimsQuery, IEnumerable<Domain.Entities.Claim>>(query, cancellationToken);
        var items = ApplySorting(claims, sort).Select(item => item.ToResponse());
        return Ok(new { items });
    }

    [HttpGet("audit-logs/list-access")]
    [AdminOnly]
    public async Task<IActionResult> GetListAccessAuditLogs(CancellationToken cancellationToken)
    {
        var query = new GetClaimsListAccessAuditLogsQuery();
        var items = (await cqrsDispatcher.ExecuteQuery<GetClaimsListAccessAuditLogsQuery, IEnumerable<Domain.Entities.ClaimAuditLogEntry>>(query, cancellationToken))
            .Select(entry => entry.ToResponse());
        return Ok(new { items });
    }

    [HttpGet("{claimId}")]
    public async Task<IActionResult> GetById(string claimId, CancellationToken cancellationToken)
    {
        var query = new GetClaimByIdQuery(claimId, GetActor());
        var result = await cqrsDispatcher.ExecuteQuery<GetClaimByIdQuery, OperationResult<Domain.Entities.Claim>>(query, cancellationToken);
        return FromResult(result, item => Ok(new { item = item.ToResponse() }));
    }

    [HttpGet("{claimId}/audit-logs")]
    public async Task<IActionResult> GetAuditLogs(string claimId, CancellationToken cancellationToken)
    {
        var query = new GetClaimAuditLogsQuery(claimId);
        var result = await cqrsDispatcher.ExecuteQuery<GetClaimAuditLogsQuery, OperationResult<IEnumerable<Domain.Entities.ClaimAuditLogEntry>>>(query, cancellationToken);
        return FromResult(result, items => Ok(new { items = items.Select(entry => entry.ToResponse()) }));
    }

    [HttpGet("status-workflow")]
    public async Task<IActionResult> GetStatusWorkflow(CancellationToken cancellationToken)
    {
        var query = new GetClaimStatusWorkflowQuery();
        var statusWorkflow = await cqrsDispatcher.ExecuteQuery<GetClaimStatusWorkflowQuery, ClaimStatusWorkflowModel>(query, cancellationToken);
        return Ok(new
        {
            createStatuses = statusWorkflow.CreateStatuses,
            workflow = statusWorkflow.Workflow.Select(item => new { status = item.Status, next = item.Next }),
        });
    }

    [HttpPost]
    [AdminOnly]
    public async Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken cancellationToken)
    {
        var candidate = new Domain.Entities.Claim
        {
            ClaimId = request.claimId,
            PolicyId = request.policyId,
            MemberName = request.memberName,
            Provider = request.provider,
            ClaimType = request.claimType,
            ServiceCategory = request.serviceCategory,
            DiagnosisCode = request.diagnosisCode,
            SubmittedAt = request.submittedAt,
            ServiceDate = request.serviceDate,
            ClaimAmount = request.claimAmount,
            Status = request.status,
            Notes = request.notes,
        };

        var command = new CreateClaimCommand(candidate, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<CreateClaimCommand, OperationResult<Domain.Entities.Claim>>(command, cancellationToken);
        return FromResult(result, item => Created($"/api/claims/{item.ClaimId}", new { item = item.ToResponse() }));
    }

    [HttpPut("{claimId}")]
    [AdminOnly]
    public async Task<IActionResult> Update(string claimId, [FromBody] UpdateClaimRequest request, CancellationToken cancellationToken)
    {
        var updates = new ClaimUpdateModel
        {
            PolicyId = request.policyId,
            MemberName = request.memberName,
            Provider = request.provider,
            ClaimType = request.claimType,
            ServiceCategory = request.serviceCategory,
            DiagnosisCode = request.diagnosisCode,
            SubmittedAt = request.submittedAt,
            ServiceDate = request.serviceDate,
            ClaimAmount = request.claimAmount,
            Status = request.status,
            Notes = request.notes,
        };

        var command = new UpdateClaimCommand(claimId, updates, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<UpdateClaimCommand, OperationResult<Domain.Entities.Claim>>(command, cancellationToken);
        return FromResult(result, item => Ok(new { item = item.ToResponse() }));
    }

    [HttpDelete("{claimId}")]
    [AdminOnly]
    public async Task<IActionResult> Delete(string claimId, CancellationToken cancellationToken)
    {
        var command = new DeleteClaimCommand(claimId, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<DeleteClaimCommand, OperationResult<bool>>(command, cancellationToken);
        return FromResult(result, _ => Ok(new { ok = true }));
    }

    private string GetActor()
    {
        var userName = User.FindFirstValue(JwtRegisteredClaimNames.Name);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (!string.IsNullOrWhiteSpace(userName))
        {
            return userName;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        return "unknown";
    }

    private static IEnumerable<Domain.Entities.Claim> ApplySorting(
        IEnumerable<Domain.Entities.Claim> items,
        IEnumerable<string>? sortTokens)
    {
        var parsedSorts = ParseSorts(sortTokens).ToList();
        if (parsedSorts.Count == 0)
        {
            return items;
        }

        IOrderedEnumerable<Domain.Entities.Claim>? ordered = null;
        foreach (var sort in parsedSorts)
        {
            ordered = ApplySort(ordered ?? items, ordered is not null, sort.Field, sort.Descending);
        }

        return ordered ?? items;
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

    private static IOrderedEnumerable<Domain.Entities.Claim> ApplySort(
        IEnumerable<Domain.Entities.Claim> source,
        bool thenBy,
        string field,
        bool descending)
    {
        return field switch
        {
            "claimid" => OrderBy(source, thenBy, descending, item => item.ClaimId),
            "policyid" => OrderBy(source, thenBy, descending, item => item.PolicyId),
            "membername" => OrderBy(source, thenBy, descending, item => item.MemberName),
            "provider" => OrderBy(source, thenBy, descending, item => item.Provider),
            "claimtype" => OrderBy(source, thenBy, descending, item => item.ClaimType),
            "servicecategory" => OrderBy(source, thenBy, descending, item => item.ServiceCategory),
            "diagnosiscode" => OrderBy(source, thenBy, descending, item => item.DiagnosisCode),
            "submittedat" => OrderBy(source, thenBy, descending, item => item.SubmittedAt),
            "servicedate" => OrderBy(source, thenBy, descending, item => item.ServiceDate),
            "claimamount" => OrderBy(source, thenBy, descending, item => item.ClaimAmount),
            "status" => OrderBy(source, thenBy, descending, item => item.Status),
            "notes" => OrderBy(source, thenBy, descending, item => item.Notes ?? string.Empty),
            _ => thenBy ? (IOrderedEnumerable<Domain.Entities.Claim>)source : source.OrderBy(item => 0),
        };
    }

    private static IOrderedEnumerable<Domain.Entities.Claim> OrderBy<TKey>(
        IEnumerable<Domain.Entities.Claim> source,
        bool thenBy,
        bool descending,
        Func<Domain.Entities.Claim, TKey> keySelector)
    {
        if (thenBy)
        {
            var ordered = (IOrderedEnumerable<Domain.Entities.Claim>)source;
            return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
        }

        return descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
    }
}
