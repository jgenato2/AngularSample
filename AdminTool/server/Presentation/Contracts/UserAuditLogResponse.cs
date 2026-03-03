namespace Server.Presentation.Contracts;

public record UserAuditLogResponse(
    string Id,
    string Action,
    string Field,
    string? OldValue,
    string? NewValue,
    string PerformedBy,
    DateTime OccurredAtUtc);
