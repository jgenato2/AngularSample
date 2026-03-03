namespace Server.Presentation.Contracts;

public record HealthInsuranceFinancialAnalyticsResponse(
    decimal AnnualPremium,
    decimal DeductibleRatio,
    decimal DeductibleLeverageIndex,
    decimal ProjectedClaimsCost,
    decimal ProjectedLossRatio,
    decimal PremiumAdequacyRatio,
    decimal TrendAdjustedClaimsCost,
    decimal VolatilityBuffer,
    decimal CapitalAtRisk95,
    decimal TailRiskRatio,
    decimal ReserveRequirement,
    decimal CombinedCapitalNeed,
    decimal SolvencyMargin,
    decimal StressScenarioCost,
    decimal StressImpact,
    decimal StressScenarioMargin,
    decimal StabilityIndex,
    int RiskScore,
    string RiskBand);
