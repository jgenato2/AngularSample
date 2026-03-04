using Server.Application.Abstractions;
using Server.Application.Features.Users.Queries;
using Server.Application.Models;
using Server.Presentation.Auditing;

namespace Server.Application.Features.Users.Handlers;

public sealed class GetUserAuditLogsQueryHandler(IUsersApplicationService usersService)
    : IQueryHandler<GetUserAuditLogsQuery, OperationResult<IEnumerable<AuditLogEntry>>>
{
    public Task<OperationResult<IEnumerable<AuditLogEntry>>> Handle(GetUserAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(usersService.GetAuditLogs(query.Id));
}
