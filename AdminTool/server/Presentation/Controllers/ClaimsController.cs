using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Server.Presentation.Auditing;
using Server.Presentation.Authorization;
using Server.Presentation.Contracts;

namespace Server.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private static readonly StringComparer StatusComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly IReadOnlyDictionary<string, string[]> StatusWorkflow =
        new Dictionary<string, string[]>(StatusComparer)
        {
            ["Submitted"] = ["Under Review", "Rejected"],
            ["Under Review"] = ["Approved", "Rejected"],
            ["Approved"] = ["Approved"],
            ["Rejected"] = ["Submitted"],
        };
    private static readonly HashSet<string> AllowedInitialStatuses = ["Submitted"];
    private static readonly object ClaimsLock = new();
    private static readonly TimeSpan ReadAuditThrottle = TimeSpan.FromMinutes(2);
    private const string AuditScope = "claims";
    private const string ListAuditClaimId = "_LIST_";
    private const int ListAuditMaxItems = 100;
    private static bool AuditSeeded;
    private static readonly List<ClaimResponse> Claims =
    [
        new(
            "CLM-2026-0001",
            "HC-2026-0001",
            "Maria Santos",
            "Blue Horizon Health",
            "Outpatient",
            "Diagnostics",
            "R51",
            new DateTime(2026, 2, 11),
            new DateTime(2026, 2, 10),
            325.75m,
            "Submitted",
            "Outpatient diagnostics"),
        new(
            "CLM-2026-0002",
            "HC-2026-0002",
            "Jared Cruz",
            "CarePlus Medical",
            "Emergency",
            "Emergency Room",
            "S06.0X0A",
            new DateTime(2026, 2, 17),
            new DateTime(2026, 2, 16),
            1420.00m,
            "Under Review",
            "Emergency room claim"),
        new(
            "CLM-2026-0003",
            "HC-2026-0003",
            "Elena Rivera",
            "WellLife Assurance",
            "Pharmacy",
            "Prescription",
            "E11.9",
            new DateTime(2026, 1, 29),
            new DateTime(2026, 1, 28),
            88.40m,
            "Approved",
            "Prescription reimbursement"),
    ];

    public ClaimsController()
    {
        EnsureAuditSeeded();
    }

    [HttpGet]
    public IActionResult List()
    {
        lock (ClaimsLock)
        {
            AddReadAuditLog(ListAuditClaimId, "ClaimList", GetActor());
            var items = Claims.OrderByDescending(claim => claim.ServiceDate).ThenBy(claim => claim.ClaimId).ToList();
            return Ok(new { items });
        }
    }

    [HttpGet("audit-logs/list-access")]
    [AdminOnly]
    public IActionResult GetListAccessAuditLogs()
    {
        lock (ClaimsLock)
        {
            var items = AuditLogStore
                .Query(AuditScope, ListAuditClaimId, ListAuditMaxItems)
                .Select(ToClaimAuditLogResponse)
                .ToList();

            return Ok(new { items });
        }
    }

    [HttpGet("{claimId}")]
    public IActionResult GetById(string claimId)
    {
        lock (ClaimsLock)
        {
            var item = Claims.FirstOrDefault(claim => claim.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return NotFound(new { message = "Claim not found." });
            }

            AddReadAuditLog(item.ClaimId, "Claim", GetActor());

            return Ok(new { item });
        }
    }

    [HttpGet("{claimId}/audit-logs")]
    public IActionResult GetAuditLogs(string claimId)
    {
        lock (ClaimsLock)
        {
            var exists = Claims.Any(claim => claim.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                return NotFound(new { message = "Claim not found." });
            }

            var items = AuditLogStore
                .Query(AuditScope, claimId)
                .Select(ToClaimAuditLogResponse)
                .ToList();

            return Ok(new { items });
        }
    }

    [HttpGet("status-workflow")]
    public IActionResult GetStatusWorkflow()
    {
        var workflow = StatusWorkflow
            .Select(entry => new
            {
                status = entry.Key,
                next = entry.Value,
            })
            .ToList();

        return Ok(new
        {
            createStatuses = AllowedInitialStatuses.OrderBy(status => status).ToArray(),
            workflow,
        });
    }

    [HttpPost]
    [AdminOnly]
    public IActionResult Create([FromBody] CreateClaimRequest request)
    {
        lock (ClaimsLock)
        {
            var normalizedStatus = NormalizeStatus(request.status);
            if (normalizedStatus is null)
            {
                return BadRequest(new { message = "Status is required." });
            }

            if (!AllowedInitialStatuses.Contains(normalizedStatus))
            {
                return BadRequest(new { message = $"Status '{request.status}' is not allowed for claim creation." });
            }

            var duplicate = Claims.Any(claim => claim.ClaimId.Equals(request.claimId, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                return Conflict(new { message = "Claim ID already exists." });
            }

            var policyAlreadyAssigned = Claims.Any(claim => claim.PolicyId.Equals(request.policyId, StringComparison.OrdinalIgnoreCase));
            if (policyAlreadyAssigned)
            {
                return Conflict(new { message = $"Policy ID '{request.policyId}' is already assigned to another claim." });
            }

            var item = new ClaimResponse(
                request.claimId,
                request.policyId,
                request.memberName,
                request.provider,
                request.claimType,
                request.serviceCategory,
                request.diagnosisCode,
                request.submittedAt,
                request.serviceDate,
                request.claimAmount,
                normalizedStatus,
                request.notes);

            Claims.Add(item);
            var actor = GetActor();
            AddAuditLog(item.ClaimId, "Created", "Claim", null, $"{item.ClaimType} ({item.Status})", actor);
            if (!string.IsNullOrWhiteSpace(item.Notes))
            {
                AddAuditLog(item.ClaimId, "Updated", "Notes", null, item.Notes, actor);
            }
            return Created($"/api/claims/{item.ClaimId}", new { item });
        }
    }

    [HttpPut("{claimId}")]
    [AdminOnly]
    public IActionResult Update(string claimId, [FromBody] UpdateClaimRequest request)
    {
        lock (ClaimsLock)
        {
            var index = Claims.FindIndex(claim => claim.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return NotFound(new { message = "Claim not found." });
            }

            var current = Claims[index];
            var nextPolicyId = request.policyId ?? current.PolicyId;

            var policyAlreadyAssigned = Claims.Any(claim =>
                !claim.ClaimId.Equals(current.ClaimId, StringComparison.OrdinalIgnoreCase)
                && claim.PolicyId.Equals(nextPolicyId, StringComparison.OrdinalIgnoreCase));
            if (policyAlreadyAssigned)
            {
                return Conflict(new { message = $"Policy ID '{nextPolicyId}' is already assigned to another claim." });
            }

            var nextStatus = current.Status;
            if (!string.IsNullOrWhiteSpace(request.status))
            {
                var normalizedStatus = NormalizeStatus(request.status);
                if (normalizedStatus is null)
                {
                    return BadRequest(new { message = "Status is required." });
                }

                if (!CanTransition(current.Status, normalizedStatus))
                {
                    return BadRequest(new { message = $"Status transition from '{current.Status}' to '{normalizedStatus}' is not allowed." });
                }

                nextStatus = normalizedStatus;
            }

            var updated = current with
            {
                PolicyId = nextPolicyId,
                MemberName = request.memberName ?? current.MemberName,
                Provider = request.provider ?? current.Provider,
                ClaimType = request.claimType ?? current.ClaimType,
                ServiceCategory = request.serviceCategory ?? current.ServiceCategory,
                DiagnosisCode = request.diagnosisCode ?? current.DiagnosisCode,
                SubmittedAt = request.submittedAt ?? current.SubmittedAt,
                ServiceDate = request.serviceDate ?? current.ServiceDate,
                ClaimAmount = request.claimAmount ?? current.ClaimAmount,
                Status = nextStatus,
                Notes = request.notes ?? current.Notes,
            };

            Claims[index] = updated;
            AddChangeAuditLogs(current, updated, GetActor());
            return Ok(new { item = updated });
        }
    }

    [HttpDelete("{claimId}")]
    [AdminOnly]
    public IActionResult Delete(string claimId)
    {
        lock (ClaimsLock)
        {
            var index = Claims.FindIndex(claim => claim.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return NotFound(new { message = "Claim not found." });
            }

            var current = Claims[index];
            AddAuditLog(current.ClaimId, "Deleted", "Claim", $"{current.ClaimType} ({current.Status})", null, GetActor());
            Claims.RemoveAt(index);
            return Ok(new { ok = true });
        }
    }

    private static void AddAuditLog(
        string claimId,
        string action,
        string field,
        string? oldValue,
        string? newValue,
        string actor)
    {
        AuditLogStore.Add(
            AuditScope,
            claimId,
            action,
            field,
            oldValue,
            newValue,
            actor);
    }

    private static void AddReadAuditLog(string claimId, string field, string actor)
    {
        AuditLogStore.AddReadWithThrottle(AuditScope, claimId, field, actor, ReadAuditThrottle);
    }

    private static ClaimAuditLogResponse ToClaimAuditLogResponse(AuditLogEntry entry)
        => new(
            entry.Id,
            entry.EntityId,
            entry.Action,
            entry.Field,
            entry.OldValue,
            entry.NewValue,
            entry.PerformedBy,
            entry.OccurredAtUtc);

    private static void EnsureAuditSeeded()
    {
        if (AuditSeeded)
        {
            return;
        }

        lock (ClaimsLock)
        {
            if (AuditSeeded)
            {
                return;
            }

            AuditLogStore.Add(AuditScope, "CLM-2026-0001", "Created", "Claim", null, "Outpatient (Submitted)", "system-seed", DateTime.UtcNow.AddDays(-30));
            AuditLogStore.Add(AuditScope, "CLM-2026-0002", "Created", "Claim", null, "Emergency (Under Review)", "system-seed", DateTime.UtcNow.AddDays(-20));
            AuditLogStore.Add(AuditScope, "CLM-2026-0003", "Created", "Claim", null, "Pharmacy (Approved)", "system-seed", DateTime.UtcNow.AddDays(-10));

            AuditSeeded = true;
        }
    }

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");

    private static string FormatDecimal(decimal value) => value.ToString("0.##");

    private void AddChangeAuditLogs(ClaimResponse current, ClaimResponse updated, string actor)
    {
        if (!string.Equals(current.PolicyId, updated.PolicyId, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "PolicyId", current.PolicyId, updated.PolicyId, actor);
        }

        if (!string.Equals(current.MemberName, updated.MemberName, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "MemberName", current.MemberName, updated.MemberName, actor);
        }

        if (!string.Equals(current.Provider, updated.Provider, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "Provider", current.Provider, updated.Provider, actor);
        }

        if (!string.Equals(current.ClaimType, updated.ClaimType, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "ClaimType", current.ClaimType, updated.ClaimType, actor);
        }

        if (!string.Equals(current.ServiceCategory, updated.ServiceCategory, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "ServiceCategory", current.ServiceCategory, updated.ServiceCategory, actor);
        }

        if (!string.Equals(current.DiagnosisCode, updated.DiagnosisCode, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "DiagnosisCode", current.DiagnosisCode, updated.DiagnosisCode, actor);
        }

        if (current.SubmittedAt.Date != updated.SubmittedAt.Date)
        {
            AddAuditLog(updated.ClaimId, "Updated", "SubmittedAt", FormatDate(current.SubmittedAt), FormatDate(updated.SubmittedAt), actor);
        }

        if (current.ServiceDate.Date != updated.ServiceDate.Date)
        {
            AddAuditLog(updated.ClaimId, "Updated", "ServiceDate", FormatDate(current.ServiceDate), FormatDate(updated.ServiceDate), actor);
        }

        if (current.ClaimAmount != updated.ClaimAmount)
        {
            AddAuditLog(updated.ClaimId, "Updated", "ClaimAmount", FormatDecimal(current.ClaimAmount), FormatDecimal(updated.ClaimAmount), actor);
        }

        if (!string.Equals(current.Status, updated.Status, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "Status", current.Status, updated.Status, actor);
        }

        if (!string.Equals(current.Notes, updated.Notes, StringComparison.Ordinal))
        {
            AddAuditLog(updated.ClaimId, "Updated", "Notes", current.Notes, updated.Notes, actor);
        }
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

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var trimmed = status.Trim();
        var known = StatusWorkflow.Keys.FirstOrDefault(value => value.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        return known;
    }

    private static bool CanTransition(string currentStatus, string nextStatus)
    {
        if (!StatusWorkflow.TryGetValue(currentStatus, out var transitions))
        {
            return false;
        }

        return transitions.Contains(nextStatus, StringComparer.OrdinalIgnoreCase);
    }
}
