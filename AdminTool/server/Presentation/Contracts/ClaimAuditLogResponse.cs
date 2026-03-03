namespace Server.Presentation.Contracts;

public record ClaimAuditLogResponse(
    string Id,
    string ClaimId,
    string Action,
    string Field,
    string? OldValue,
    string? NewValue,
    string PerformedBy,
    DateTime OccurredAtUtc);
