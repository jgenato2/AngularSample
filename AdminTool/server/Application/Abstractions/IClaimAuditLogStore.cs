using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IClaimAuditLogStore
{
    void Add(string claimId, string action, string field, string? oldValue, string? newValue, string actor, DateTime? occurredAtUtc = null);
    void AddReadWithThrottle(string claimId, string field, string actor, TimeSpan throttle, DateTime? now = null);
    IEnumerable<ClaimAuditLogEntry> Query(string claimId, int? take = null);
}