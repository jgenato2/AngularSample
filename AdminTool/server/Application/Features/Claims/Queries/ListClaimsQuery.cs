using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Queries;

public sealed record ListClaimsQuery(string Actor) : IQuery<IEnumerable<Claim>>;
