using Server.Application.Abstractions;
using Server.Application.Features.Users.Queries;
using Server.Presentation.Auditing;

namespace Server.Application.Features.Users.Handlers;

public sealed class GetUsersListAccessAuditLogsQueryHandler
    : IQueryHandler<GetUsersListAccessAuditLogsQuery, IEnumerable<AuditLogEntry>>
{
    private const string AuditScope = "users";
    private const string ListAuditEntityId = "_LIST_";
    private const int ListAuditMaxItems = 100;

    public Task<IEnumerable<AuditLogEntry>> Handle(GetUsersListAccessAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<AuditLogEntry>>(AuditLogStore.Query(AuditScope, ListAuditEntityId, ListAuditMaxItems));
}
