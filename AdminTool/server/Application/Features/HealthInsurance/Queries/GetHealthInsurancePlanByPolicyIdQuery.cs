using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Queries;

public sealed record GetHealthInsurancePlanByPolicyIdQuery(string PolicyId, string Actor)
    : IQuery<OperationResult<HealthInsurancePlanResponse>>;
