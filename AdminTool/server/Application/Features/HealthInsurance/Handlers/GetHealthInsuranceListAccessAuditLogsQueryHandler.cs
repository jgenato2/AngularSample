using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Presentation.Auditing;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class GetHealthInsuranceListAccessAuditLogsQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<GetHealthInsuranceListAccessAuditLogsQuery, IEnumerable<AuditLogEntry>>
{
    public Task<IEnumerable<AuditLogEntry>> Handle(GetHealthInsuranceListAccessAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(service.GetListAccessAuditLogs());
}
