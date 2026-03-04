namespace Server.Domain.Entities;

public sealed class ClaimAuditLogEntry
{
    public string Id { get; init; } = "";
    public string ClaimId { get; init; } = "";
    public string Action { get; init; } = "";
    public string Field { get; init; } = "";
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string PerformedBy { get; init; } = "";
    public DateTime OccurredAtUtc { get; init; }
}