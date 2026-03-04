using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Infrastructure.Persistence;

public sealed class ClaimAuditLogStore : IClaimAuditLogStore
{
    private static readonly object Sync = new();
    private static readonly List<ClaimAuditLogEntry> Entries = [];

    public void Add(
        string claimId,
        string action,
        string field,
        string? oldValue,
        string? newValue,
        string actor,
        DateTime? occurredAtUtc = null)
    {
        lock (Sync)
        {
            Entries.Add(new ClaimAuditLogEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                ClaimId = claimId,
                Action = action,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue,
                PerformedBy = actor,
                OccurredAtUtc = occurredAtUtc ?? DateTime.UtcNow,
            });
        }
    }

    public void AddReadWithThrottle(string claimId, string field, string actor, TimeSpan throttle, DateTime? now = null)
    {
        lock (Sync)
        {
            var current = now ?? DateTime.UtcNow;
            var duplicate = Entries.Any(entry =>
                entry.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.Action, "Viewed", StringComparison.Ordinal)
                && string.Equals(entry.Field, field, StringComparison.Ordinal)
                && string.Equals(entry.PerformedBy, actor, StringComparison.Ordinal)
                && current - entry.OccurredAtUtc <= throttle);

            if (duplicate)
            {
                return;
            }

            Entries.Add(new ClaimAuditLogEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                ClaimId = claimId,
                Action = "Viewed",
                Field = field,
                OldValue = null,
                NewValue = null,
                PerformedBy = actor,
                OccurredAtUtc = current,
            });
        }
    }

    public IEnumerable<ClaimAuditLogEntry> Query(string claimId, int? take = null)
    {
        lock (Sync)
        {
            IEnumerable<ClaimAuditLogEntry> query = Entries
                .Where(entry => entry.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(entry => entry.OccurredAtUtc);

            if (take is > 0)
            {
                query = query.Take(take.Value);
            }

            return query.Select(Clone).ToList();
        }
    }

    private static ClaimAuditLogEntry Clone(ClaimAuditLogEntry entry)
        => new()
        {
            Id = entry.Id,
            ClaimId = entry.ClaimId,
            Action = entry.Action,
            Field = entry.Field,
            OldValue = entry.OldValue,
            NewValue = entry.NewValue,
            PerformedBy = entry.PerformedBy,
            OccurredAtUtc = entry.OccurredAtUtc,
        };
}