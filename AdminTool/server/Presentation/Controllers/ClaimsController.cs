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
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var query = new ListClaimsQuery(GetActor());
        var items = (await cqrsDispatcher.ExecuteQuery<ListClaimsQuery, IEnumerable<Domain.Entities.Claim>>(query, cancellationToken)).Select(item => item.ToResponse());
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
}
