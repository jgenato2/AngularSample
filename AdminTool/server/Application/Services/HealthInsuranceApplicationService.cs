using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Auditing;
using Server.Presentation.Contracts;

namespace Server.Application.Services;

public sealed class HealthInsuranceApplicationService(
    IHealthInsuranceSeedService seedService,
    IHealthInsuranceWorkflowService workflowService,
    IHealthInsuranceAnalyticsService analyticsService,
    IHealthInsuranceAuditService auditService) : IHealthInsuranceApplicationService
{
    private static readonly object PlansLock = new();
    private static readonly object InitializationLock = new();
    private static bool Initialized;
    private static readonly List<HealthInsurancePlanResponse> Plans =
    [
        new(
            "HC-2026-0001",
            "Maria Santos",
            "Blue Horizon Health",
            "Family PPO",
            420.50m,
            1500m,
            6500m,
            "Active",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            "Family coverage with annual wellness incentives."),
        new(
            "HC-2026-0002",
            "Jared Cruz",
            "CarePlus Medical",
            "Individual HMO",
            275.00m,
            1000m,
            4500m,
            "Active",
            new DateTime(2026, 2, 1),
            new DateTime(2027, 1, 31),
            "Primary care-centric plan with referral requirement."),
        new(
            "HC-2026-0003",
            "Elena Rivera",
            "WellLife Assurance",
            "Senior Advantage",
            198.75m,
            500m,
            3200m,
            "Pending Renewal",
            new DateTime(2025, 4, 15),
            new DateTime(2026, 4, 14),
            "Renewal pending member confirmation."),
    ];

    public void Initialize() => EnsureInitialized();

    public IEnumerable<HealthInsurancePlanResponse> ListPlans(string actor)
    {
        lock (PlansLock)
        {
            auditService.AddListRead(actor);
            return Plans.OrderBy(plan => plan.PolicyId).ToList();
        }
    }

    public Task<OperationResult<HealthInsurancePlanResponse>> GetByPolicyId(string policyId, string actor)
    {
        lock (PlansLock)
        {
            var item = Plans.FirstOrDefault(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail("Insurance plan not found.", ErrorType.NotFound));
            }

            auditService.AddPlanRead(item.PolicyId, actor);
            return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Ok(item));
        }
    }

    public Task<OperationResult<HealthInsuranceFinancialAnalyticsResponse>> GetFinancialAnalytics(string policyId, string actor)
    {
        lock (PlansLock)
        {
            var item = Plans.FirstOrDefault(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return Task.FromResult(OperationResult<HealthInsuranceFinancialAnalyticsResponse>.Fail("Insurance plan not found.", ErrorType.NotFound));
            }

            auditService.AddFinancialAnalyticsRead(item.PolicyId, actor);
            return Task.FromResult(OperationResult<HealthInsuranceFinancialAnalyticsResponse>.Ok(analyticsService.Build(item)));
        }
    }

    public Task<OperationResult<IEnumerable<AuditLogEntry>>> GetAuditLogs(string policyId)
    {
        lock (PlansLock)
        {
            var exists = Plans.Any(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                return Task.FromResult(OperationResult<IEnumerable<AuditLogEntry>>.Fail("Insurance plan not found.", ErrorType.NotFound));
            }

            return Task.FromResult(OperationResult<IEnumerable<AuditLogEntry>>.Ok(auditService.GetAuditLogs(policyId)));
        }
    }

    public IEnumerable<AuditLogEntry> GetListAccessAuditLogs()
    {
        lock (PlansLock)
        {
            return auditService.GetListAccessAuditLogs().ToList();
        }
    }

    public IEnumerable<AuditLogEntry> GetAllAuditLogs()
    {
        lock (PlansLock)
        {
            return auditService.GetAllAuditLogs().ToList();
        }
    }

    public InsuranceStatusWorkflowModel GetStatusWorkflow()
        => workflowService.GetStatusWorkflow();

    public Task<OperationResult<HealthInsurancePlanResponse>> Create(CreateHealthInsurancePlanRequest request, string actor)
    {
        lock (PlansLock)
        {
            var normalizedStatus = workflowService.NormalizeStatus(request.status);
            if (normalizedStatus is null)
            {
                return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail("Invalid insurance status.", ErrorType.Validation));
            }

            if (!workflowService.AllowedInitialStatuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
            {
                return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail(
                    $"Status '{normalizedStatus}' is not allowed when creating a plan. Allowed values: {string.Join(", ", workflowService.AllowedInitialStatuses)}.",
                    ErrorType.Validation));
            }

            var duplicate = Plans.Any(plan => plan.PolicyId.Equals(request.policyId, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail("Policy ID already exists.", ErrorType.Conflict));
            }

            var item = new HealthInsurancePlanResponse(
                request.policyId,
                request.memberName,
                request.provider,
                request.planType,
                request.monthlyPremium,
                request.deductible,
                request.outOfPocketMax,
                normalizedStatus,
                request.effectiveDate,
                request.renewalDate,
                request.comments);

            Plans.Add(item);
            auditService.AddPlanCreated(item, actor);
            return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Ok(item));
        }
    }

    public Task<OperationResult<HealthInsurancePlanResponse>> Update(string policyId, UpdateHealthInsurancePlanRequest request, string actor)
    {
        lock (PlansLock)
        {
            var index = Plans.FindIndex(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail("Insurance plan not found.", ErrorType.NotFound));
            }

            var current = Plans[index];
            string? nextStatus = null;

            if (!string.IsNullOrWhiteSpace(request.status))
            {
                nextStatus = workflowService.NormalizeStatus(request.status);
                if (nextStatus is null)
                {
                    return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail("Invalid insurance status.", ErrorType.Validation));
                }

                if (string.Equals(current.Status, nextStatus, StringComparison.OrdinalIgnoreCase))
                {
                    nextStatus = current.Status;
                }
                else if (!workflowService.CanTransition(current.Status, nextStatus))
                {
                    return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Fail(
                        $"Invalid status transition from '{current.Status}' to '{nextStatus}'.",
                        ErrorType.Validation));
                }
            }

            var updated = current with
            {
                MemberName = request.memberName ?? current.MemberName,
                Provider = request.provider ?? current.Provider,
                PlanType = request.planType ?? current.PlanType,
                MonthlyPremium = request.monthlyPremium ?? current.MonthlyPremium,
                Deductible = request.deductible ?? current.Deductible,
                OutOfPocketMax = request.outOfPocketMax ?? current.OutOfPocketMax,
                Status = nextStatus ?? current.Status,
                EffectiveDate = request.effectiveDate ?? current.EffectiveDate,
                RenewalDate = request.renewalDate ?? current.RenewalDate,
                Comments = request.comments ?? current.Comments,
            };

            Plans[index] = updated;
            auditService.AddChangeAuditLogs(current, updated, actor);
            return Task.FromResult(OperationResult<HealthInsurancePlanResponse>.Ok(updated));
        }
    }

    public Task<OperationResult<bool>> Delete(string policyId, string actor)
    {
        lock (PlansLock)
        {
            var index = Plans.FindIndex(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return Task.FromResult(OperationResult<bool>.Fail("Insurance plan not found.", ErrorType.NotFound));
            }

            var current = Plans[index];
            auditService.AddPlanDeleted(current, actor);
            Plans.RemoveAt(index);
            return Task.FromResult(OperationResult<bool>.Ok(true));
        }
    }

    private void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        lock (InitializationLock)
        {
            if (Initialized)
            {
                return;
            }

            seedService.EnsureSeeded(Plans);
            auditService.EnsureSeeded();
            Initialized = true;
        }
    }
}
