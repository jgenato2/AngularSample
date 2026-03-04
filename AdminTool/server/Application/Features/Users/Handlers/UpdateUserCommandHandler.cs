using Server.Application.Abstractions;
using Server.Application.Features.Users.Commands;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Handlers;

public sealed class UpdateUserCommandHandler(IUsersApplicationService usersService)
    : ICommandHandler<UpdateUserCommand, OperationResult<User>>
{
    public Task<OperationResult<User>> Handle(UpdateUserCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(usersService.Update(command.Id, command.Updates, command.AllowRole, command.Actor));
}
