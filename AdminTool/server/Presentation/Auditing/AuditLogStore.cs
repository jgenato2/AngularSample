namespace Server.Presentation.Auditing;

public record AuditLogEntry(
    string Id,
    string Scope,
    string EntityId,
    string Action,
    string Field,
    string? OldValue,
    string? NewValue,
    string PerformedBy,
    DateTime OccurredAtUtc);

public static class AuditLogStore
{
    private static readonly object Sync = new();
    private static readonly List<AuditLogEntry> Entries = [];

    public static void Add(
        string scope,
        string entityId,
        string action,
        string field,
        string? oldValue,
        string? newValue,
        string performedBy,
        DateTime? occurredAtUtc = null)
    {
        lock (Sync)
        {
            Entries.Add(new AuditLogEntry(
                Guid.NewGuid().ToString("N"),
                scope,
                entityId,
                action,
                field,
                oldValue,
                newValue,
                performedBy,
                occurredAtUtc ?? DateTime.UtcNow));
        }
    }

    public static void AddReadWithThrottle(
        string scope,
        string entityId,
        string field,
        string performedBy,
        TimeSpan throttle,
        DateTime? now = null)
    {
        lock (Sync)
        {
            var current = now ?? DateTime.UtcNow;
            var duplicate = Entries.Any(entry =>
                entry.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase)
                && entry.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "Viewed", StringComparison.Ordinal)
                && string.Equals(entry.Field, field, StringComparison.Ordinal)
                && string.Equals(entry.PerformedBy, performedBy, StringComparison.Ordinal)
                && current - entry.OccurredAtUtc <= throttle);

            if (duplicate)
            {
                return;
            }

            Entries.Add(new AuditLogEntry(
                Guid.NewGuid().ToString("N"),
                scope,
                entityId,
                "Viewed",
                field,
                null,
                null,
                performedBy,
                current));
        }
    }

    public static List<AuditLogEntry> Query(
        string scope,
        string? entityId = null,
        int? take = null)
    {
        lock (Sync)
        {
            IEnumerable<AuditLogEntry> query = Entries
                .Where(entry => entry.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(entityId))
            {
                query = query.Where(entry => entry.EntityId.Equals(entityId, StringComparison.OrdinalIgnoreCase));
            }

            query = query.OrderByDescending(entry => entry.OccurredAtUtc);

            if (take is > 0)
            {
                query = query.Take(take.Value);
            }

            return query.ToList();
        }
    }
}
