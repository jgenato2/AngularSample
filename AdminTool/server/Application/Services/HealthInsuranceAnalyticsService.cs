using Server.Application.Abstractions;
using Server.Presentation.Contracts;

namespace Server.Application.Services;

public sealed class HealthInsuranceAnalyticsService(IConfiguration configuration) : IHealthInsuranceAnalyticsService
{
    private readonly HealthInsuranceAnalyticsConfig analyticsConfig =
        configuration.GetSection("HealthInsuranceAnalytics").Get<HealthInsuranceAnalyticsConfig>()
        ?? new HealthInsuranceAnalyticsConfig();

    public HealthInsuranceFinancialAnalyticsResponse Build(HealthInsurancePlanResponse plan)
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
