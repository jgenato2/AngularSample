using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Queries;

public sealed record GetAllClaimsAuditLogsQuery : IQuery<IEnumerable<ClaimAuditLogEntry>>;
