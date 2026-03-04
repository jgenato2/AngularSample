using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Features.Claims.Queries;

public sealed record GetClaimStatusWorkflowQuery : IQuery<ClaimStatusWorkflowModel>;
