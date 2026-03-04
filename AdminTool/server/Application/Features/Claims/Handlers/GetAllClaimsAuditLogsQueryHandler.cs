using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class GetAllClaimsAuditLogsQueryHandler(IClaimsApplicationService claimsService)
    : IQueryHandler<GetAllClaimsAuditLogsQuery, IEnumerable<ClaimAuditLogEntry>>
{
    public Task<IEnumerable<ClaimAuditLogEntry>> Handle(GetAllClaimsAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.GetAllAuditLogs());
}
