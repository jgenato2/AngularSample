using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Commands;
using Server.Application.Models;
using Server.Presentation.Contracts;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class CreateHealthInsurancePlanCommandHandler(IHealthInsuranceApplicationService service)
    : ICommandHandler<CreateHealthInsurancePlanCommand, OperationResult<HealthInsurancePlanResponse>>
{
    public Task<OperationResult<HealthInsurancePlanResponse>> Handle(CreateHealthInsurancePlanCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(service.Create(command.Request, command.Actor));
}
