using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Auditing;

namespace Server.Application.Features.HealthInsurance.Queries;

public sealed record GetHealthInsuranceAuditLogsQuery(string PolicyId)
    : IQuery<OperationResult<IEnumerable<AuditLogEntry>>>;
