using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class ListHealthInsurancePlansQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<ListHealthInsurancePlansQuery, IEnumerable<HealthInsurancePlanResponse>>
{
    public Task<IEnumerable<HealthInsurancePlanResponse>> Handle(ListHealthInsurancePlansQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(service.ListPlans(query.Actor));
}
