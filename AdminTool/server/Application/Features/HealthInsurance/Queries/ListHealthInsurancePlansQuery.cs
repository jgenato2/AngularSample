using Server.Application.Abstractions;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Queries;

public sealed record ListHealthInsurancePlansQuery(string Actor)
    : IQuery<IEnumerable<HealthInsurancePlanResponse>>;
