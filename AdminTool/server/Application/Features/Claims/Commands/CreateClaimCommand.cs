using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Commands;

public sealed record CreateClaimCommand(Claim Claim, string Actor) : ICommand<OperationResult<Claim>>;
