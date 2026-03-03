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
[Route("api/health-insurance")]
public class HealthInsuranceController : ControllerBase
{
    private readonly HealthInsuranceAnalyticsConfig analyticsConfig;
    private static readonly StringComparer StatusComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly IReadOnlyDictionary<string, string[]> StatusWorkflow =
        new Dictionary<string, string[]>(StatusComparer)
        {
            ["New"] = ["Underwriting", "Cancelled"],
            ["Underwriting"] = ["Pending Activation", "Cancelled"],
            ["Pending Activation"] = ["Active", "Cancelled"],
            ["Active"] = ["Grace Period", "Pending Renewal", "Suspended", "Cancelled", "Expired"],
            ["Grace Period"] = ["Active", "Suspended", "Cancelled", "Expired"],
            ["Pending Renewal"] = ["Renewed", "Expired", "Cancelled"],
            ["Renewed"] = ["Active", "Cancelled"],
            ["Suspended"] = ["Active", "Cancelled", "Expired"],
            ["Cancelled"] = ["New"],
            ["Expired"] = ["New"],
        };
    private static readonly HashSet<string> AllowedInitialStatuses = ["New"];
    private static readonly object PlansLock = new();
    private static readonly TimeSpan ReadAuditThrottle = TimeSpan.FromMinutes(2);
    private const string ListAuditPolicyId = "_LIST_";
    private const int ListAuditMaxItems = 100;
    private const string AuditScope = "insurance";
    private static bool AuditSeeded;
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

    public HealthInsuranceController(IConfiguration configuration)
    {
        analyticsConfig = configuration.GetSection("HealthInsuranceAnalytics").Get<HealthInsuranceAnalyticsConfig>()
            ?? new HealthInsuranceAnalyticsConfig();
        EnsureAuditSeeded();
    }

    [HttpGet("plans")]
    public IActionResult ListPlans()
    {
        lock (PlansLock)
        {
            AddReadAuditLog(ListAuditPolicyId, "PlanList", GetActor());
            var items = Plans.OrderBy(plan => plan.PolicyId).ToList();
            return Ok(new { items });
        }
    }

    [HttpGet("plans/{policyId}")]
    public IActionResult GetByPolicyId(string policyId)
    {
        lock (PlansLock)
        {
            var item = Plans.FirstOrDefault(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            AddReadAuditLog(item.PolicyId, "Plan", GetActor());

            return Ok(new { item });
        }
    }

    [HttpGet("plans/{policyId}/financial-analytics")]
    public IActionResult GetFinancialAnalytics(string policyId)
    {
        lock (PlansLock)
        {
            var item = Plans.FirstOrDefault(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            AddReadAuditLog(item.PolicyId, "FinancialAnalytics", GetActor());
            var analytics = BuildFinancialAnalytics(item);
            return Ok(new { item = analytics });
        }
    }

    [HttpGet("plans/{policyId}/audit-logs")]
    public IActionResult GetAuditLogs(string policyId)
    {
        lock (PlansLock)
        {
            var exists = Plans.Any(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            var items = AuditLogStore
                .Query(AuditScope, policyId)
                .Select(ToInsuranceAuditLogResponse)
                .ToList();

            return Ok(new { items });
        }
    }

    [HttpGet("audit-logs/list-access")]
    [AdminOnly]
    public IActionResult GetListAccessAuditLogs()
    {
        lock (PlansLock)
        {
            var items = AuditLogStore
                .Query(AuditScope, ListAuditPolicyId, ListAuditMaxItems)
                .Select(ToInsuranceAuditLogResponse)
                .ToList();

            return Ok(new { items });
        }
    }

    [HttpGet("status-workflow")]
    public IActionResult GetStatusWorkflow()
    {
        var workflow = StatusWorkflow
            .OrderBy(item => item.Key)
            .Select(item => new { status = item.Key, next = item.Value })
            .ToList();

        return Ok(new
        {
            createStatuses = AllowedInitialStatuses.OrderBy(value => value).ToList(),
            workflow,
        });
    }

    [HttpPost("plans")]
    [AdminOnly]
    public IActionResult Create([FromBody] CreateHealthInsurancePlanRequest request)
    {
        lock (PlansLock)
        {
            var normalizedStatus = NormalizeStatus(request.status);
            if (normalizedStatus is null)
            {
                return BadRequest(new { message = "Invalid insurance status." });
            }

            if (!AllowedInitialStatuses.Contains(normalizedStatus))
            {
                return BadRequest(new
                {
                    message = $"Status '{normalizedStatus}' is not allowed when creating a plan. Allowed values: {string.Join(", ", AllowedInitialStatuses)}.",
                });
            }

            var duplicate = Plans.Any(plan => plan.PolicyId.Equals(request.policyId, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                return Conflict(new { message = "Policy ID already exists." });
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
            var actor = GetActor();
            AddAuditLog(item.PolicyId, "Created", "Plan", null, $"{item.PlanType} ({item.Status})", actor);
            if (!string.IsNullOrWhiteSpace(item.Comments))
            {
                AddAuditLog(item.PolicyId, "Updated", "Comments", null, item.Comments, actor);
            }
            return Created($"/api/health-insurance/plans/{item.PolicyId}", new { item });
        }
    }

    [HttpPut("plans/{policyId}")]
    [AdminOnly]
    public IActionResult Update(string policyId, [FromBody] UpdateHealthInsurancePlanRequest request)
    {
        lock (PlansLock)
        {
            var index = Plans.FindIndex(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            var current = Plans[index];
            string? nextStatus = null;
            if (!string.IsNullOrWhiteSpace(request.status))
            {
                nextStatus = NormalizeStatus(request.status);
                if (nextStatus is null)
                {
                    return BadRequest(new { message = "Invalid insurance status." });
                }

                var canTransition = CanTransition(current.Status, nextStatus);
                if (!canTransition)
                {
                    return BadRequest(new
                    {
                        message = $"Invalid status transition from '{current.Status}' to '{nextStatus}'.",
                        allowedNextStatuses = GetAllowedNextStatuses(current.Status),
                    });
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
            var updateActor = GetActor();
            AddChangeAuditLogs(current, updated, updateActor);
            return Ok(new { item = updated });
        }
    }

    [HttpDelete("plans/{policyId}")]
    [AdminOnly]
    public IActionResult Delete(string policyId)
    {
        lock (PlansLock)
        {
            var index = Plans.FindIndex(plan => plan.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return NotFound(new { message = "Insurance plan not found." });
            }

            var current = Plans[index];
            AddAuditLog(current.PolicyId, "Deleted", "Plan", $"{current.PlanType} ({current.Status})", null, GetActor());
            Plans.RemoveAt(index);
            return Ok(new { ok = true });
        }
    }

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

    private static void AddReadAuditLog(string policyId, string field, string actor)
    {
        AuditLogStore.AddReadWithThrottle(AuditScope, policyId, field, actor, ReadAuditThrottle);
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

    private static void EnsureAuditSeeded()
    {
        if (AuditSeeded)
        {
            return;
        }

        lock (PlansLock)
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

    private static string FormatDate(DateTime value) => value.ToString("yyyy-MM-dd");

    private static string FormatDecimal(decimal value) => value.ToString("0.##");

    private void AddChangeAuditLogs(HealthInsurancePlanResponse current, HealthInsurancePlanResponse updated, string actor)
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

    private HealthInsuranceFinancialAnalyticsResponse BuildFinancialAnalytics(HealthInsurancePlanResponse plan)
    {
        var annualPremium = plan.MonthlyPremium * 12m;
        var deductibleRatio = plan.OutOfPocketMax <= 0m ? 0m : plan.Deductible / plan.OutOfPocketMax;

        var planFactor = plan.PlanType switch
        {
            "Family PPO" => analyticsConfig.PlanFactors.FamilyPpo,
            "Individual HMO" => analyticsConfig.PlanFactors.IndividualHmo,
            "Senior Advantage" => analyticsConfig.PlanFactors.SeniorAdvantage,
            _ => analyticsConfig.PlanFactors.Default,
        };

        var statusFactor = plan.Status switch
        {
            "Active" => analyticsConfig.StatusFactors.Active,
            "Pending Renewal" => analyticsConfig.StatusFactors.PendingRenewal,
            "Expired" => analyticsConfig.StatusFactors.Expired,
            _ => analyticsConfig.StatusFactors.Default,
        };

        var deductibleRelief = Math.Max(analyticsConfig.DeductibleReliefMinimum, 1m - (deductibleRatio * analyticsConfig.DeductibleReliefWeight));
        var projectedClaimsCost = annualPremium * planFactor * statusFactor * deductibleRelief;
        var projectedLossRatio = annualPremium <= 0m ? 0m : projectedClaimsCost / annualPremium * 100m;
        var premiumAdequacyRatio = projectedClaimsCost <= 0m ? 0m : annualPremium / projectedClaimsCost * 100m;
        var trendAdjustedClaimsCost = projectedClaimsCost * (1m + analyticsConfig.MedicalInflationRate + analyticsConfig.UtilizationTrendRate);
        var lossRatioNormalized = Math.Max(0m, projectedLossRatio / 100m);
        var volatilityBuffer = trendAdjustedClaimsCost * analyticsConfig.ClaimVolatilityBase * (1m + (decimal)Math.Sqrt((double)lossRatioNormalized));
        var capitalAtRisk95 = trendAdjustedClaimsCost + volatilityBuffer;
        var tailRiskRatio = annualPremium <= 0m ? 0m : capitalAtRisk95 / annualPremium * 100m;
        var reserveRequirement = Math.Max(analyticsConfig.ReserveRequirementMinimum, projectedClaimsCost * analyticsConfig.ReserveRequirementRate);
        var solvencyMargin = annualPremium - projectedClaimsCost - reserveRequirement;
        var stressScenarioCost = projectedClaimsCost * analyticsConfig.StressScenarioMultiplier;
        var stressImpact = stressScenarioCost - projectedClaimsCost;
        var stressScenarioMargin = annualPremium - stressScenarioCost - reserveRequirement;
        var combinedCapitalNeed = reserveRequirement + Math.Max(0m, -stressScenarioMargin);
        var deductibleLeverageIndex = (1m - Math.Min(1m, deductibleRatio)) * 100m;
        var stabilityRaw = analyticsConfig.StabilityBaseScore
            + ((premiumAdequacyRatio - 100m) * analyticsConfig.StabilityAdequacyWeight)
            - (tailRiskRatio * analyticsConfig.StabilityTailRiskWeight)
            - (Math.Abs(stressScenarioMargin) / Math.Max(1m, annualPremium) * 100m * analyticsConfig.StabilityStressWeight);
        var stabilityIndex = Math.Clamp(stabilityRaw, 0m, 100m);

        var lossRatioScore = Math.Min(analyticsConfig.LossRatioScoreCap, projectedLossRatio * analyticsConfig.LossRatioScoreWeight);
        var deductibleScore = Math.Max(0m, (1m - deductibleRatio) * analyticsConfig.DeductibleScoreWeight);
        var statusPenalty = plan.Status == "Pending Renewal" ? analyticsConfig.PendingRenewalPenalty : 0m;
        var rawRiskScore = lossRatioScore + deductibleScore + statusPenalty;
        var riskScore = (int)Math.Clamp(Math.Round(rawRiskScore, MidpointRounding.AwayFromZero), analyticsConfig.RiskScoreMinimum, analyticsConfig.RiskScoreMaximum);

        var riskBand = riskScore >= analyticsConfig.HighRiskThreshold
            ? "High"
            : riskScore >= analyticsConfig.ModerateRiskThreshold
                ? "Moderate"
                : "Low";

        return new HealthInsuranceFinancialAnalyticsResponse(
            AnnualPremium: Math.Round(annualPremium, 2),
            DeductibleRatio: Math.Round(deductibleRatio, 4),
            DeductibleLeverageIndex: Math.Round(deductibleLeverageIndex, 2),
            ProjectedClaimsCost: Math.Round(projectedClaimsCost, 2),
            ProjectedLossRatio: Math.Round(projectedLossRatio, 2),
            PremiumAdequacyRatio: Math.Round(premiumAdequacyRatio, 2),
            TrendAdjustedClaimsCost: Math.Round(trendAdjustedClaimsCost, 2),
            VolatilityBuffer: Math.Round(volatilityBuffer, 2),
            CapitalAtRisk95: Math.Round(capitalAtRisk95, 2),
            TailRiskRatio: Math.Round(tailRiskRatio, 2),
            ReserveRequirement: Math.Round(reserveRequirement, 2),
            CombinedCapitalNeed: Math.Round(combinedCapitalNeed, 2),
            SolvencyMargin: Math.Round(solvencyMargin, 2),
            StressScenarioCost: Math.Round(stressScenarioCost, 2),
            StressImpact: Math.Round(stressImpact, 2),
            StressScenarioMargin: Math.Round(stressScenarioMargin, 2),
            StabilityIndex: Math.Round(stabilityIndex, 2),
            RiskScore: riskScore,
            RiskBand: riskBand);
    }

    private static string? NormalizeStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (StatusComparer.Equals(trimmed, "Draft"))
        {
            return "New";
        }

        var match = StatusWorkflow.Keys.FirstOrDefault(status => StatusComparer.Equals(status, trimmed));
        return match;
    }

    private static bool CanTransition(string currentStatus, string nextStatus)
    {
        var normalizedCurrentStatus = NormalizeStatus(currentStatus) ?? currentStatus;
        var normalizedNextStatus = NormalizeStatus(nextStatus) ?? nextStatus;

        if (StatusComparer.Equals(normalizedCurrentStatus, normalizedNextStatus))
        {
            return true;
        }

        if (!StatusWorkflow.TryGetValue(normalizedCurrentStatus, out var allowedNextStatuses))
        {
            return false;
        }

        return allowedNextStatuses.Any(status => StatusComparer.Equals(status, normalizedNextStatus));
    }

    private static IReadOnlyList<string> GetAllowedNextStatuses(string currentStatus)
    {
        var normalizedCurrentStatus = NormalizeStatus(currentStatus) ?? currentStatus;
        if (!StatusWorkflow.TryGetValue(normalizedCurrentStatus, out var allowedNextStatuses))
        {
            return [];
        }

        return allowedNextStatuses;
    }

    public sealed class HealthInsuranceAnalyticsConfig
    {
        public PlanFactorsConfig PlanFactors { get; set; } = new();
        public StatusFactorsConfig StatusFactors { get; set; } = new();
        public decimal DeductibleReliefMinimum { get; set; } = 0.65m;
        public decimal DeductibleReliefWeight { get; set; } = 0.35m;
        public decimal ReserveRequirementMinimum { get; set; } = 750m;
        public decimal ReserveRequirementRate { get; set; } = 0.18m;
        public decimal StressScenarioMultiplier { get; set; } = 1.12m;
        public decimal LossRatioScoreCap { get; set; } = 60m;
        public decimal LossRatioScoreWeight { get; set; } = 0.55m;
        public decimal DeductibleScoreWeight { get; set; } = 25m;
        public decimal PendingRenewalPenalty { get; set; } = 8m;
        public decimal MedicalInflationRate { get; set; } = 0.06m;
        public decimal UtilizationTrendRate { get; set; } = 0.03m;
        public decimal ClaimVolatilityBase { get; set; } = 0.12m;
        public decimal StabilityBaseScore { get; set; } = 65m;
        public decimal StabilityAdequacyWeight { get; set; } = 0.25m;
        public decimal StabilityTailRiskWeight { get; set; } = 0.35m;
        public decimal StabilityStressWeight { get; set; } = 0.4m;
        public decimal RiskScoreMinimum { get; set; } = 5m;
        public decimal RiskScoreMaximum { get; set; } = 99m;
        public int HighRiskThreshold { get; set; } = 75;
        public int ModerateRiskThreshold { get; set; } = 45;
    }

    public sealed class PlanFactorsConfig
    {
        public decimal FamilyPpo { get; set; } = 1.15m;
        public decimal IndividualHmo { get; set; } = 0.95m;
        public decimal SeniorAdvantage { get; set; } = 1.35m;
        public decimal Default { get; set; } = 1m;
    }

    public sealed class StatusFactorsConfig
    {
        public decimal Active { get; set; } = 1m;
        public decimal PendingRenewal { get; set; } = 1.08m;
        public decimal Expired { get; set; } = 1.2m;
        public decimal Default { get; set; } = 1m;
    }

}
