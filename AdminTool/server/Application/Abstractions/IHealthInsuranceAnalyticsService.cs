using Server.Presentation.Contracts;

namespace Server.Application.Abstractions;

public interface IHealthInsuranceAnalyticsService
{
    HealthInsuranceFinancialAnalyticsResponse Build(HealthInsurancePlanResponse plan);
}
