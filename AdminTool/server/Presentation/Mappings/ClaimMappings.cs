using Server.Domain.Entities;
using Server.Presentation.Contracts;

namespace Server.Presentation.Mappings;

public static class ClaimMappings
{
    public static ClaimResponse ToResponse(this Claim claim)
        => new(
            claim.ClaimId,
            claim.PolicyId,
            claim.MemberName,
            claim.Provider,
            claim.ClaimType,
            claim.ServiceCategory,
            claim.DiagnosisCode,
            claim.SubmittedAt,
            claim.ServiceDate,
            claim.ClaimAmount,
            claim.Status,
            claim.Notes);

    public static ClaimAuditLogResponse ToResponse(this ClaimAuditLogEntry entry)
        => new(
            entry.Id,
            entry.ClaimId,
            entry.Action,
            entry.Field,
            entry.OldValue,
            entry.NewValue,
            entry.PerformedBy,
            entry.OccurredAtUtc);
}