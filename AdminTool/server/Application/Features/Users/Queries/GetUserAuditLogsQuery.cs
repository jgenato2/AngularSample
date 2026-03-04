using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Auditing;

namespace Server.Application.Features.Users.Queries;

public sealed record GetUserAuditLogsQuery(string Id)
    : IQuery<OperationResult<IEnumerable<AuditLogEntry>>>;
