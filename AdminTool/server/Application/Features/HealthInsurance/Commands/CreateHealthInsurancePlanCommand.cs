using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Commands;

public sealed record CreateHealthInsurancePlanCommand(CreateHealthInsurancePlanRequest Request, string Actor)
    : ICommand<OperationResult<HealthInsurancePlanResponse>>;
