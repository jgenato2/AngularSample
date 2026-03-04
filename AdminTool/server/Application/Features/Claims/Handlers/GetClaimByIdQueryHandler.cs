using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class GetClaimByIdQueryHandler(IClaimsApplicationService claimsService)
    : IQueryHandler<GetClaimByIdQuery, OperationResult<Claim>>
{
    public Task<OperationResult<Claim>> Handle(GetClaimByIdQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.GetById(query.ClaimId, query.Actor));
}
