using Server.Application.Abstractions;
using Server.Application.Features.Auth.Commands;
using Server.Application.Models;

namespace Server.Application.Features.Auth.Handlers;

public sealed class RegisterCommandHandler(IAuthApplicationService authService)
    : ICommandHandler<RegisterCommand, OperationResult<AuthPayload>>
{
    public Task<OperationResult<AuthPayload>> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
        => Task.FromResult(authService.Register(command.Name, command.Email, command.Password));
}
