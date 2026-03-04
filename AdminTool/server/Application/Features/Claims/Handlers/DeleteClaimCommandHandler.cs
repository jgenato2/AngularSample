using Server.Application.Abstractions;
using Server.Application.Features.Claims.Commands;
using Server.Application.Models;

namespace Server.Application.Features.Claims.Handlers;

public sealed class DeleteClaimCommandHandler(IClaimsApplicationService claimsService)
    : ICommandHandler<DeleteClaimCommand, OperationResult<bool>>
{
    public Task<OperationResult<bool>> Handle(DeleteClaimCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.Delete(command.ClaimId, command.Actor));
}
