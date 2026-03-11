using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Models;
using Server.Presentation.Auditing;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class GetHealthInsuranceAuditLogsQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<GetHealthInsuranceAuditLogsQuery, OperationResult<IEnumerable<AuditLogEntry>>>
{
    public Task<OperationResult<IEnumerable<AuditLogEntry>>> Handle(GetHealthInsuranceAuditLogsQuery query, CancellationToken cancellationToken = default)
        => service.GetAuditLogs(query.PolicyId);
}
