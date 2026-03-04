using Server.Application.Abstractions;
using Server.Application.Features.Users.Commands;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Handlers;

public sealed class CreateUserCommandHandler(IUsersApplicationService usersService)
    : ICommandHandler<CreateUserCommand, OperationResult<User>>
{
    public Task<OperationResult<User>> Handle(CreateUserCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(usersService.Create(command.Name, command.Email, command.Role, command.Password, command.Actor));
}
