using Server.Application.Abstractions;
using Server.Application.Features.Claims.Commands;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class CreateClaimCommandHandler(IClaimsApplicationService claimsService)
    : ICommandHandler<CreateClaimCommand, OperationResult<Claim>>
{
    public Task<OperationResult<Claim>> Handle(CreateClaimCommand command, CancellationToken cancellationToken = default)
        => claimsService.Create(command.Claim, command.Actor);
}
