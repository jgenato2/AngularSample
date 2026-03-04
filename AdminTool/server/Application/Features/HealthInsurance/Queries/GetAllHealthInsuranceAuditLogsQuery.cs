using Server.Application.Abstractions;
using Server.Presentation.Auditing;

namespace Server.Application.Features.HealthInsurance.Queries;

public sealed record GetAllHealthInsuranceAuditLogsQuery()
    : IQuery<IEnumerable<AuditLogEntry>>;
