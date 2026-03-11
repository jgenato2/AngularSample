using Server.Application.Abstractions;
using Server.Application.Features.Users.Commands;
using Server.Application.Models;

namespace Server.Application.Features.Users.Handlers;

public sealed class DeleteUserCommandHandler(IUsersApplicationService usersService)
    : ICommandHandler<DeleteUserCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(DeleteUserCommand command, CancellationToken cancellationToken = default)
        => await usersService.Delete(command.Id, command.Actor);
}
