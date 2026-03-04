using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Commands;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Models;
using Server.Presentation.Auditing;
using Server.Presentation.Authorization;
using Server.Presentation.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/health-insurance")]
public class HealthInsuranceController(ICqrsDispatcher cqrsDispatcher) : ApiControllerBase
{
    [HttpGet("plans")]
    public async Task<IActionResult> ListPlans(CancellationToken cancellationToken)
    {
        var query = new ListHealthInsurancePlansQuery(GetActor());
        var items = await cqrsDispatcher.ExecuteQuery<ListHealthInsurancePlansQuery, IEnumerable<HealthInsurancePlanResponse>>(query, cancellationToken);
        return Ok(new { items });
    }

    [HttpGet("plans/{policyId}")]
    public async Task<IActionResult> GetByPolicyId(string policyId, CancellationToken cancellationToken)
    {
        var query = new GetHealthInsurancePlanByPolicyIdQuery(policyId, GetActor());
        var result = await cqrsDispatcher.ExecuteQuery<GetHealthInsurancePlanByPolicyIdQuery, OperationResult<HealthInsurancePlanResponse>>(query, cancellationToken);
        return FromResult(result, item => Ok(new { item }));
    }

    [HttpGet("plans/{policyId}/financial-analytics")]
    public async Task<IActionResult> GetFinancialAnalytics(string policyId, CancellationToken cancellationToken)
    {
        var query = new GetHealthInsuranceFinancialAnalyticsQuery(policyId, GetActor());
        var result = await cqrsDispatcher.ExecuteQuery<GetHealthInsuranceFinancialAnalyticsQuery, OperationResult<HealthInsuranceFinancialAnalyticsResponse>>(query, cancellationToken);
        return FromResult(result, item => Ok(new { item }));
    }

    [HttpGet("plans/{policyId}/audit-logs")]
    public async Task<IActionResult> GetAuditLogs(string policyId, CancellationToken cancellationToken)
    {
        var query = new GetHealthInsuranceAuditLogsQuery(policyId);
        var result = await cqrsDispatcher.ExecuteQuery<GetHealthInsuranceAuditLogsQuery, OperationResult<IEnumerable<AuditLogEntry>>>(query, cancellationToken);
        return FromResult(result, entries => Ok(new { items = entries.Select(ToInsuranceAuditLogResponse) }));
    }

    [HttpGet("audit-logs/list-access")]
    [AdminOnly]
    public async Task<IActionResult> GetListAccessAuditLogs(CancellationToken cancellationToken)
    {
        var query = new GetHealthInsuranceListAccessAuditLogsQuery();
        var items = (await cqrsDispatcher.ExecuteQuery<GetHealthInsuranceListAccessAuditLogsQuery, IEnumerable<AuditLogEntry>>(query, cancellationToken))
            .Select(ToInsuranceAuditLogResponse)
            .ToList();

        return Ok(new { items });
    }

    [HttpGet("status-workflow")]
    public async Task<IActionResult> GetStatusWorkflow(CancellationToken cancellationToken)
    {
        var query = new GetHealthInsuranceStatusWorkflowQuery();
        var workflow = await cqrsDispatcher.ExecuteQuery<GetHealthInsuranceStatusWorkflowQuery, InsuranceStatusWorkflowModel>(query, cancellationToken);

        return Ok(new
        {
            createStatuses = workflow.CreateStatuses,
            workflow = workflow.Workflow.Select(item => new { status = item.Status, next = item.Next }),
        });
    }

    [HttpPost("plans")]
    [AdminOnly]
    public async Task<IActionResult> Create([FromBody] CreateHealthInsurancePlanRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateHealthInsurancePlanCommand(request, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<CreateHealthInsurancePlanCommand, OperationResult<HealthInsurancePlanResponse>>(command, cancellationToken);
        return FromResult(result, item => Created($"/api/health-insurance/plans/{item.PolicyId}", new { item }));
    }

    [HttpPut("plans/{policyId}")]
    [AdminOnly]
    public async Task<IActionResult> Update(string policyId, [FromBody] UpdateHealthInsurancePlanRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateHealthInsurancePlanCommand(policyId, request, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<UpdateHealthInsurancePlanCommand, OperationResult<HealthInsurancePlanResponse>>(command, cancellationToken);
        return FromResult(result, item => Ok(new { item }));
    }

    [HttpDelete("plans/{policyId}")]
    [AdminOnly]
    public async Task<IActionResult> Delete(string policyId, CancellationToken cancellationToken)
    {
        var command = new DeleteHealthInsurancePlanCommand(policyId, GetActor());
        var result = await cqrsDispatcher.ExecuteCommand<DeleteHealthInsurancePlanCommand, OperationResult<bool>>(command, cancellationToken);
        return FromResult(result, _ => Ok(new { ok = true }));
    }

    private static HealthInsuranceAuditLogResponse ToInsuranceAuditLogResponse(AuditLogEntry entry)
        => new(
            entry.Id,
            entry.EntityId,
            entry.Action,
            entry.Field,
            entry.OldValue,
            entry.NewValue,
            entry.PerformedBy,
            entry.OccurredAtUtc);

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
