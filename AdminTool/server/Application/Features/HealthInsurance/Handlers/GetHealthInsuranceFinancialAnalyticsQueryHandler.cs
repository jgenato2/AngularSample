using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Queries;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class GetHealthInsuranceFinancialAnalyticsQueryHandler(IHealthInsuranceApplicationService service)
    : IQueryHandler<GetHealthInsuranceFinancialAnalyticsQuery, OperationResult<HealthInsuranceFinancialAnalyticsResponse>>
{
    public Task<OperationResult<HealthInsuranceFinancialAnalyticsResponse>> Handle(GetHealthInsuranceFinancialAnalyticsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(service.GetFinancialAnalytics(query.PolicyId, query.Actor));
}
