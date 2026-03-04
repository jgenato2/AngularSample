using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class GetClaimAuditLogsQueryHandler(IClaimsApplicationService claimsService)
    : IQueryHandler<GetClaimAuditLogsQuery, OperationResult<IEnumerable<ClaimAuditLogEntry>>>
{
    public Task<OperationResult<IEnumerable<ClaimAuditLogEntry>>> Handle(GetClaimAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.GetAuditLogs(query.ClaimId));
}
