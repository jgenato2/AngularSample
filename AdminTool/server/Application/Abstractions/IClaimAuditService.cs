using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IClaimAuditService
{
    void EnsureSeeded();
    void AddListRead(string actor);
    void AddClaimRead(string claimId, string actor);
    void AddClaimCreated(Claim claim, string actor);
    void AddClaimDeleted(Claim claim, string actor);
    void AddChangeAuditLogs(Claim current, Claim updated, string actor);
    IEnumerable<ClaimAuditLogEntry> GetAuditLogs(string claimId);
    IEnumerable<ClaimAuditLogEntry> GetListAccessAuditLogs();
}
