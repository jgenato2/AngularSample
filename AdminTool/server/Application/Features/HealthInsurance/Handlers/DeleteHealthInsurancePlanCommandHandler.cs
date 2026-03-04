using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Commands;
using Server.Application.Models;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class DeleteHealthInsurancePlanCommandHandler(IHealthInsuranceApplicationService service)
    : ICommandHandler<DeleteHealthInsurancePlanCommand, OperationResult<bool>>
{
    public Task<OperationResult<bool>> Handle(DeleteHealthInsurancePlanCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(service.Delete(command.PolicyId, command.Actor));
}
