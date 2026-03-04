using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Queries;

public sealed record GetClaimByIdQuery(string ClaimId, string Actor) : IQuery<OperationResult<Claim>>;
