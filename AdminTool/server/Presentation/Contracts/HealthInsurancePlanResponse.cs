namespace Server.Presentation.Contracts;

public record HealthInsurancePlanResponse(
    string PolicyId,
    string MemberName,
    string Provider,
    string PlanType,
    decimal MonthlyPremium,
    decimal Deductible,
    decimal OutOfPocketMax,
    string Status,
    DateTime EffectiveDate,
    DateTime RenewalDate,
    string? Comments);
