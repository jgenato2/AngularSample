using Server.Application.Abstractions;
using Server.Application.Features.Users.Queries;
using Server.Presentation.Auditing;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Handlers;

public sealed class ListUsersQueryHandler(IUsersApplicationService usersService)
    : IQueryHandler<ListUsersQuery, IEnumerable<User>>
{
    private static readonly TimeSpan ReadAuditThrottle = TimeSpan.FromMinutes(2);
    private const string AuditScope = "users";
    private const string ListAuditEntityId = "_LIST_";

    public Task<IEnumerable<User>> Handle(ListUsersQuery query, CancellationToken cancellationToken = default)
    {
        AuditLogStore.AddReadWithThrottle(AuditScope, ListAuditEntityId, "UserList", query.Actor, ReadAuditThrottle);
        return Task.FromResult(usersService.List());
    }
}
