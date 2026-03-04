using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Features.HealthInsurance.Queries;

public sealed record GetHealthInsuranceStatusWorkflowQuery()
    : IQuery<InsuranceStatusWorkflowModel>;
