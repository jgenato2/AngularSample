using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class GetHealthInsurancePlanByPolicyIdQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<GetHealthInsurancePlanByPolicyIdQuery, OperationResult<HealthInsurancePlanResponse>>
{
    public Task<OperationResult<HealthInsurancePlanResponse>> Handle(GetHealthInsurancePlanByPolicyIdQuery query, CancellationToken cancellationToken = default)
        => service.GetByPolicyId(query.PolicyId, query.Actor);
}
