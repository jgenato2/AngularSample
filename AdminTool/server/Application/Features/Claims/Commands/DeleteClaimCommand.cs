using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Features.Claims.Commands;

public sealed record DeleteClaimCommand(string ClaimId, string Actor) : ICommand<OperationResult<bool>>;
