namespace Server.Presentation.Contracts;

public record HealthInsuranceAuditLogResponse(
    string Id,
    string PolicyId,
    string Action,
    string Field,
    string? OldValue,
    string? NewValue,
    string PerformedBy,
    DateTime OccurredAtUtc);
