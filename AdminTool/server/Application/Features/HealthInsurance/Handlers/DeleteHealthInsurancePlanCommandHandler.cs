using Server.Application.Abstractions;
using Server.Application.Features.HealthInsurance.Commands;
using Server.Application.Models;

namespace Server.Application.Features.HealthInsurance.Handlers;

public sealed class DeleteHealthInsurancePlanCommandHandler(IHealthInsuranceApplicationService service)
    : ICommandHandler<DeleteHealthInsurancePlanCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(DeleteHealthInsurancePlanCommand command, CancellationToken cancellationToken = default)
        => await service.Delete(command.PolicyId, command.Actor);
}
