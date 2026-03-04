using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Commands;

public sealed record UpdateClaimCommand(string ClaimId, ClaimUpdateModel Updates, string Actor) : ICommand<OperationResult<Claim>>;
