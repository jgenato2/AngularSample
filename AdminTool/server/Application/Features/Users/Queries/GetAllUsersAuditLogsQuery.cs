using Server.Application.Abstractions;
using Server.Presentation.Auditing;

namespace Server.Application.Features.Users.Queries;

public sealed record GetAllUsersAuditLogsQuery() : IQuery<IEnumerable<AuditLogEntry>>;
