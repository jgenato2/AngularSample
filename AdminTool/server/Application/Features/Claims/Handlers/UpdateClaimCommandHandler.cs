using Server.Application.Abstractions;
using Server.Application.Features.Claims.Commands;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class UpdateClaimCommandHandler(IClaimsApplicationService claimsService)
    : ICommandHandler<UpdateClaimCommand, OperationResult<Claim>>
{
    public Task<OperationResult<Claim>> Handle(UpdateClaimCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.Update(command.ClaimId, command.Updates, command.Actor));
}
