using Server.Application.Abstractions;
using Server.Presentation.Auditing;
using Server.Presentation.Contracts;

namespace Server.Application.Services;

public sealed class HealthInsuranceAuditService : IHealthInsuranceAuditService
{
    private static readonly TimeSpan ReadAuditThrottle = TimeSpan.FromMinutes(2);
    private const string ListAuditPolicyId = "_LIST_";
    private const int ListAuditMaxItems = 100;
    private const string AuditScope = "insurance";
    private static bool AuditSeeded;
    private static readonly object AuditSeedLock = new();

    public void EnsureSeeded()
    {
        if (AuditSeeded)
        {
            return;
        }

        lock (AuditSeedLock)
        {
            if (AuditSeeded)
            {
                return;
            }

            AuditLogStore.Add(AuditScope, "HC-2026-0001", "Created", "Plan", null, "Family PPO (Active)", "system-seed", DateTime.UtcNow.AddDays(-45));
            AuditLogStore.Add(AuditScope, "HC-2026-0002", "Created", "Plan", null, "Individual HMO (Active)", "system-seed", DateTime.UtcNow.AddDays(-30));
            AuditLogStore.Add(AuditScope, "HC-2026-0003", "Created", "Plan", null, "Senior Advantage (Pending Renewal)", "system-seed", DateTime.UtcNow.AddDays(-20));

            AuditSeeded = true;
        }
    }

    public void AddListRead(string actor)
        => AuditLogStore.AddReadWithThrottle(AuditScope, ListAuditPolicyId, "PlanList", actor, ReadAuditThrottle);

    public void AddPlanRead(string policyId, string actor)
        => AuditLogStore.AddReadWithThrottle(AuditScope, policyId, "Plan", actor, ReadAuditThrottle);

    public void AddFinancialAnalyticsRead(string policyId, string actor)
        => AuditLogStore.AddReadWithThrottle(AuditScope, policyId, "FinancialAnalytics", actor, ReadAuditThrottle);

    public void AddPlanCreated(HealthInsurancePlanResponse item, string actor)
    {
        AddAuditLog(item.PolicyId, "Created", "Plan", null, $"{item.PlanType} ({item.Status})", actor);
        if (!string.IsNullOrWhiteSpace(item.Comments))
        {
            AddAuditLog(item.PolicyId, "Updated", "Comments", null, item.Comments, actor);
        }
    }

    public void AddPlanDeleted(HealthInsurancePlanResponse item, string actor)
        => AddAuditLog(item.PolicyId, "Deleted", "Plan", $"{item.PlanType} ({item.Status})", null, actor);

    public void AddChangeAuditLogs(HealthInsurancePlanResponse current, HealthInsurancePlanResponse updated, string actor)
    {
        if (!string.Equals(current.MemberName, updated.MemberName, StringComparison.Ordinal))
        {
            AddAuditLog(updated.PolicyId, "Updated", "MemberName", current.MemberName, updated.MemberName, actor);
        }

        if (!string.Equals(current.Provider, updated.Provider, StringComparison.Ordinal))
        {
            AddAuditLog(updated.PolicyId, "Updated", "Provider", current.Provider, updated.Provider, actor);
        }

        if (!string.Equals(current.PlanType, updated.PlanType, StringComparison.Ordinal))
        {
            AddAuditLog(updated.PolicyId, "Updated", "PlanType", current.PlanType, updated.PlanType, actor);
        }

        if (current.MonthlyPremium != updated.MonthlyPremium)
        {
            AddAuditLog(updated.PolicyId, "Updated", "MonthlyPremium", FormatDecimal(current.MonthlyPremium), FormatDecimal(updated.MonthlyPremium), actor);
        }

        if (current.Deductible != updated.Deductible)
        {
            AddAuditLog(updated.PolicyId, "Updated", "Deductible", FormatDecimal(current.Deductible), FormatDecimal(updated.Deductible), actor);
        }

        if (current.OutOfPocketMax != updated.OutOfPocketMax)
        {
            AddAuditLog(updated.PolicyId, "Updated", "OutOfPocketMax", FormatDecimal(current.OutOfPocketMax), FormatDecimal(updated.OutOfPocketMax), actor);
        }

        if (!string.Equals(current.Status, updated.Status, StringComparison.Ordinal))
        {
            AddAuditLog(updated.PolicyId, "Updated", "Status", current.Status, updated.Status, actor);
        }

        if (current.EffectiveDate.Date != updated.EffectiveDate.Date)
        {
            AddAuditLog(updated.PolicyId, "Updated", "EffectiveDate", FormatDate(current.EffectiveDate), FormatDate(updated.EffectiveDate), actor);
        }

        if (current.RenewalDate.Date != updated.RenewalDate.Date)
        {
            AddAuditLog(updated.PolicyId, "Updated", "RenewalDate", FormatDate(current.RenewalDate), FormatDate(updated.RenewalDate), actor);
        }

        if (!string.Equals(current.Comments, updated.Comments, StringComparison.Ordinal))
        {
            AddAuditLog(updated.PolicyId, "Updated", "Comments", current.Comments, updated.Comments, actor);
        }
    }

    public IEnumerable<AuditLogEntry> GetAuditLogs(string policyId)
        => AuditLogStore.Query(AuditScope, policyId);

    public IEnumerable<AuditLogEntry> GetListAccessAuditLogs()
        => AuditLogStore.Query(AuditScope, ListAuditPolicyId, ListAuditMaxItems);

    public IEnumerable<AuditLogEntry> GetAllAuditLogs()
        => AuditLogStore.Query(AuditScope);

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");

    private static string FormatDecimal(decimal value) => value.ToString("0.##");

    private static void AddAuditLog(
        string policyId,
        string action,
        string field,
        string? oldValue,
        string? newValue,
        string actor)
    {
        AuditLogStore.Add(
            AuditScope,
            policyId,
            action,
            field,
            oldValue,
            newValue,
            actor);
    }
}
