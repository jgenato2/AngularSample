using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Commands;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class UpdateHealthInsurancePlanCommandHandler(IHealthInsuranceApplicationService service)
    : ICommandHandler<UpdateHealthInsurancePlanCommand, OperationResult<HealthInsurancePlanResponse>>
{
    public Task<OperationResult<HealthInsurancePlanResponse>> Handle(UpdateHealthInsurancePlanCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(service.Update(command.PolicyId, command.Request, command.Actor));
}
