using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Features.HealthInsurance.Commands;

public sealed record DeleteHealthInsurancePlanCommand(string PolicyId, string Actor)
    : ICommand<OperationResult<bool>>;
