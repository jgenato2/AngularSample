using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IClaimsStore
{
    IEnumerable<Claim> List();
    Claim? FindById(string claimId);
    bool ClaimIdExists(string claimId);
    bool PolicyAssignedToOtherClaim(string policyId, string? excludedClaimId = null);
    void Add(Claim claim);
    void Update(Claim claim);
    bool Delete(string claimId, out Claim? removed);
    void SeedIfEmpty(IEnumerable<Claim> claims);
}