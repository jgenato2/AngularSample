using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Presentation.Auditing;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class GetAllHealthInsuranceAuditLogsQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<GetAllHealthInsuranceAuditLogsQuery, IEnumerable<AuditLogEntry>>
{
    public Task<IEnumerable<AuditLogEntry>> Handle(GetAllHealthInsuranceAuditLogsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(service.GetAllAuditLogs());
}
