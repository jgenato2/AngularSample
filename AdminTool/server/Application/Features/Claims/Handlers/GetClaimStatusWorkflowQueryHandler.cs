using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Application.Models;

namespace Server.Application.Features.Claims.Handlers;

public sealed class GetClaimStatusWorkflowQueryHandler(IClaimsApplicationService claimsService)
    : IQueryHandler<GetClaimStatusWorkflowQuery, ClaimStatusWorkflowModel>
{
    public Task<ClaimStatusWorkflowModel> Handle(GetClaimStatusWorkflowQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.GetStatusWorkflow());
}
