using Server.Application.Abstractions;
using Server.Application.Features.Auth.Commands;
using Server.Application.Models;

namespace Server.Application.Features.Auth.Handlers;

public sealed class LoginCommandHandler(IAuthApplicationService authService)
    : ICommandHandler<LoginCommand, OperationResult<AuthPayload>>
{
    public Task<OperationResult<AuthPayload>> Handle(LoginCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(authService.Login(command.Email, command.Password));
}
