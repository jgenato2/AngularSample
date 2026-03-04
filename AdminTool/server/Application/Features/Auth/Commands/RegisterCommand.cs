using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Features.Auth.Commands;

public sealed record RegisterCommand(string Name, string Email, string Password)
    : ICommand<OperationResult<AuthPayload>>;
