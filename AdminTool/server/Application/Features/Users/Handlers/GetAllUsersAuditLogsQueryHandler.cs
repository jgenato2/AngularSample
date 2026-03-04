using Server.Application.Abstractions;
using Server.Application.Features.Users.Queries;
using Server.Presentation.Auditing;

namespace Server.Application.Features.Users.Handlers;

public sealed class GetAllUsersAuditLogsQueryHandler(IUsersApplicationService usersService)
    : IQueryHandler<GetAllUsersAuditLogsQuery, IEnumerable<AuditLogEntry>>
{
    public Task<IEnumerable<AuditLogEntry>> Handle(GetAllUsersAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(usersService.GetAllAuditLogs());
}
