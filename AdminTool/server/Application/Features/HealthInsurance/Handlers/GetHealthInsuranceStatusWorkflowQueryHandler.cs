using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Models;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class GetHealthInsuranceStatusWorkflowQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<GetHealthInsuranceStatusWorkflowQuery, InsuranceStatusWorkflowModel>
{
    public Task<InsuranceStatusWorkflowModel> Handle(GetHealthInsuranceStatusWorkflowQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(service.GetStatusWorkflow());
}
