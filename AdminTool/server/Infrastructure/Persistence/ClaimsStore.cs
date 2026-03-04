using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Infrastructure.Persistence;

public sealed class ClaimsStore : IClaimsStore
{
    private static readonly object Sync = new();
    private static readonly List<Claim> Claims = [];

    public IEnumerable<Claim> List()
    {
        lock (Sync)
        {
            return Claims.Select(Clone).ToList();
        }
    }

    public Claim? FindById(string claimId)
    {
        lock (Sync)
        {
            var match = Claims.FirstOrDefault(claim => claim.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
            return match is null ? null : Clone(match);
        }
    }

    public bool ClaimIdExists(string claimId)
    {
        lock (Sync)
        {
            return Claims.Any(claim => claim.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool PolicyAssignedToOtherClaim(string policyId, string? excludedClaimId = null)
    {
        lock (Sync)
        {
            return Claims.Any(claim =>
                claim.PolicyId.Equals(policyId, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(excludedClaimId) || !claim.ClaimId.Equals(excludedClaimId, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public void Add(Claim claim)
    {
        lock (Sync)
        {
            Claims.Add(Clone(claim));
        }
    }

    public void Update(Claim claim)
    {
        lock (Sync)
        {
            var index = Claims.FindIndex(item => item.ClaimId.Equals(claim.ClaimId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return;
            }

            Claims[index] = Clone(claim);
        }
    }

    public bool Delete(string claimId, out Claim? removed)
    {
        lock (Sync)
        {
            var index = Claims.FindIndex(item => item.ClaimId.Equals(claimId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                removed = null;
                return false;
            }

            removed = Clone(Claims[index]);
            Claims.RemoveAt(index);
            return true;
        }
    }

    public void SeedIfEmpty(IEnumerable<Claim> claims)
    {
        lock (Sync)
        {
            if (Claims.Count > 0)
            {
                return;
            }

            Claims.AddRange(claims.Select(Clone));
        }
    }

    private static Claim Clone(Claim claim)
        => new()
        {
            ClaimId = claim.ClaimId,
            PolicyId = claim.PolicyId,
            MemberName = claim.MemberName,
            Provider = claim.Provider,
            ClaimType = claim.ClaimType,
            ServiceCategory = claim.ServiceCategory,
            DiagnosisCode = claim.DiagnosisCode,
            SubmittedAt = claim.SubmittedAt,
            ServiceDate = claim.ServiceDate,
            ClaimAmount = claim.ClaimAmount,
            Status = claim.Status,
            Notes = claim.Notes,
        };
}