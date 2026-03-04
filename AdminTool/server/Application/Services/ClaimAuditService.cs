using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Application.Services;

public sealed class ClaimAuditService(IClaimAuditLogStore auditLogStore) : IClaimAuditService
{
    private static readonly TimeSpan ReadAuditThrottle = TimeSpan.FromMinutes(2);
    private const string ListAuditClaimId = "_LIST_";
    private const int ListAuditMaxItems = 100;
    private static bool Seeded;
    private static readonly object SeedLock = new();

    public void EnsureSeeded()
    {
        if (Seeded)
        {
            return;
        }

        lock (SeedLock)
        {
            if (Seeded)
            {
                return;
            }

            auditLogStore.Add("CLM-2026-0001", "Created", "Claim", null, "Outpatient (Submitted)", "system-seed", DateTime.UtcNow.AddDays(-30));
            auditLogStore.Add("CLM-2026-0002", "Created", "Claim", null, "Emergency (Under Review)", "system-seed", DateTime.UtcNow.AddDays(-20));
            auditLogStore.Add("CLM-2026-0003", "Created", "Claim", null, "Pharmacy (Approved)", "system-seed", DateTime.UtcNow.AddDays(-10));

            Seeded = true;
        }
    }

    public void AddListRead(string actor)
        => auditLogStore.AddReadWithThrottle(ListAuditClaimId, "ClaimList", actor, ReadAuditThrottle);

    public void AddClaimRead(string claimId, string actor)
        => auditLogStore.AddReadWithThrottle(claimId, "Claim", actor, ReadAuditThrottle);

    public void AddClaimCreated(Claim claim, string actor)
    {
        auditLogStore.Add(claim.ClaimId, "Created", "Claim", null, $"{claim.ClaimType} ({claim.Status})", actor);
        if (!string.IsNullOrWhiteSpace(claim.Notes))
        {
            auditLogStore.Add(claim.ClaimId, "Updated", "Notes", null, claim.Notes, actor);
        }
    }

    public void AddClaimDeleted(Claim claim, string actor)
        => auditLogStore.Add(claim.ClaimId, "Deleted", "Claim", $"{claim.ClaimType} ({claim.Status})", null, actor);

    public IEnumerable<ClaimAuditLogEntry> GetAuditLogs(string claimId)
        => auditLogStore.Query(claimId);

    public IEnumerable<ClaimAuditLogEntry> GetListAccessAuditLogs()
        => auditLogStore.Query(ListAuditClaimId, ListAuditMaxItems);

    public void AddChangeAuditLogs(Claim current, Claim updated, string actor)
    {
        if (!string.Equals(current.PolicyId, updated.PolicyId, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "PolicyId", current.PolicyId, updated.PolicyId, actor);
        }

        if (!string.Equals(current.MemberName, updated.MemberName, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "MemberName", current.MemberName, updated.MemberName, actor);
        }

        if (!string.Equals(current.Provider, updated.Provider, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "Provider", current.Provider, updated.Provider, actor);
        }

        if (!string.Equals(current.ClaimType, updated.ClaimType, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "ClaimType", current.ClaimType, updated.ClaimType, actor);
        }

        if (!string.Equals(current.ServiceCategory, updated.ServiceCategory, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "ServiceCategory", current.ServiceCategory, updated.ServiceCategory, actor);
        }

        if (!string.Equals(current.DiagnosisCode, updated.DiagnosisCode, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "DiagnosisCode", current.DiagnosisCode, updated.DiagnosisCode, actor);
        }

        if (current.SubmittedAt.Date != updated.SubmittedAt.Date)
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "SubmittedAt", FormatDate(current.SubmittedAt), FormatDate(updated.SubmittedAt), actor);
        }

        if (current.ServiceDate.Date != updated.ServiceDate.Date)
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "ServiceDate", FormatDate(current.ServiceDate), FormatDate(updated.ServiceDate), actor);
        }

        if (current.ClaimAmount != updated.ClaimAmount)
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "ClaimAmount", FormatDecimal(current.ClaimAmount), FormatDecimal(updated.ClaimAmount), actor);
        }

        if (!string.Equals(current.Status, updated.Status, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "Status", current.Status, updated.Status, actor);
        }

        if (!string.Equals(current.Notes, updated.Notes, StringComparison.Ordinal))
        {
            auditLogStore.Add(updated.ClaimId, "Updated", "Notes", current.Notes, updated.Notes, actor);
        }
    }

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");

    private static string FormatDecimal(decimal value) => value.ToString("0.##");
}
