using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Commands;

public sealed record UpdateHealthInsurancePlanCommand(string PolicyId, UpdateHealthInsurancePlanRequest Request, string Actor)
    : ICommand<OperationResult<HealthInsurancePlanResponse>>;
