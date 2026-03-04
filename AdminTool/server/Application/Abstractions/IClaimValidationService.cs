using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IClaimValidationService
{
    OperationResult<Claim> ValidateCreate(Claim claim);
    OperationResult<Claim> ValidateAndBuildUpdated(Claim current, ClaimUpdateModel updates);
}
