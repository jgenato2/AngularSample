using Server.Application.Abstractions;
using Server.Application.Features.Claims.Queries;
using Server.Domain.Entities;

namespace Server.Application.Features.Claims.Handlers;

public sealed class ListClaimsQueryHandler(IClaimsApplicationService claimsService)
    : IQueryHandler<ListClaimsQuery, IEnumerable<Claim>>
{
    public Task<IEnumerable<Claim>> Handle(ListClaimsQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(claimsService.List(query.Actor));
}
