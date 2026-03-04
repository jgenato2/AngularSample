using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Features.Users.Commands;

public sealed record DeleteUserCommand(string Id, string Actor) : ICommand<OperationResult<bool>>;
