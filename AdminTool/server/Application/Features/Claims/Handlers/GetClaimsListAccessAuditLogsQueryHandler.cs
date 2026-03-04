using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class GetClaimsListAccessAuditLogsQueryHandler(IClaimsApplicationService claimsService)
    : IQueryHandler<GetClaimsListAccessAuditLogsQuery, IEnumerable<ClaimAuditLogEntry>>
{
    public Task<IEnumerable<ClaimAuditLogEntry>> Handle(GetClaimsListAccessAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.GetListAccessAuditLogs());
}
