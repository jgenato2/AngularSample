using Server.Application.Abstractions;
using Server.Application.Features.Users.Queries;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Handlers;

public sealed class GetUserByIdQueryHandler(IUsersApplicationService usersService)
    : IQueryHandler<GetUserByIdQuery, OperationResult<User>>
{
    public Task<OperationResult<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(usersService.GetById(query.Id));
}
