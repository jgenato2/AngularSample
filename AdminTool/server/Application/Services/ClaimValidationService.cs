using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Services;

public sealed class ClaimValidationService(IClaimsStore claimsStore, IClaimWorkflowService workflowService) : IClaimValidationService
{
    public OperationResult<Claim> ValidateCreate(Claim claim)
    {
        var normalizedStatus = workflowService.NormalizeStatus(claim.Status);
        if (normalizedStatus is null)
        {
            return OperationResult<Claim>.Fail("Status is required.", ErrorType.Validation);
        }

        if (!workflowService.AllowedInitialStatuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult<Claim>.Fail($"Status '{claim.Status}' is not allowed for claim creation.", ErrorType.Validation);
        }

        if (claimsStore.ClaimIdExists(claim.ClaimId))
        {
            return OperationResult<Claim>.Fail("Claim ID already exists.", ErrorType.Conflict);
        }

        if (claimsStore.PolicyAssignedToOtherClaim(claim.PolicyId))
        {
            return OperationResult<Claim>.Fail($"Policy ID '{claim.PolicyId}' is already assigned to another claim.", ErrorType.Conflict);
        }

        claim.Status = normalizedStatus;
        return OperationResult<Claim>.Ok(claim);
    }

    public OperationResult<Claim> ValidateAndBuildUpdated(Claim current, ClaimUpdateModel updates)
    {
        var nextPolicyId = updates.PolicyId ?? current.PolicyId;
        if (claimsStore.PolicyAssignedToOtherClaim(nextPolicyId, current.ClaimId))
        {
            return OperationResult<Claim>.Fail($"Policy ID '{nextPolicyId}' is already assigned to another claim.", ErrorType.Conflict);
        }

        var nextStatus = current.Status;
        if (!string.IsNullOrWhiteSpace(updates.Status))
        {
            var normalizedStatus = workflowService.NormalizeStatus(updates.Status);
            if (normalizedStatus is null)
            {
                return OperationResult<Claim>.Fail("Status is required.", ErrorType.Validation);
            }

            if (!workflowService.CanTransition(current.Status, normalizedStatus))
            {
                return OperationResult<Claim>.Fail($"Status transition from '{current.Status}' to '{normalizedStatus}' is not allowed.", ErrorType.Validation);
            }

            nextStatus = normalizedStatus;
        }

        var updated = new Claim
        {
            ClaimId = current.ClaimId,
            PolicyId = nextPolicyId,
            MemberName = updates.MemberName ?? current.MemberName,
            Provider = updates.Provider ?? current.Provider,
            ClaimType = updates.ClaimType ?? current.ClaimType,
            ServiceCategory = updates.ServiceCategory ?? current.ServiceCategory,
            DiagnosisCode = updates.DiagnosisCode ?? current.DiagnosisCode,
            SubmittedAt = updates.SubmittedAt ?? current.SubmittedAt,
            ServiceDate = updates.ServiceDate ?? current.ServiceDate,
            ClaimAmount = updates.ClaimAmount ?? current.ClaimAmount,
            Status = nextStatus,
            Notes = updates.Notes ?? current.Notes,
        };

        return OperationResult<Claim>.Ok(updated);
    }
}
