using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Commands;

public sealed record CreateUserCommand(string Name, string Email, string Role, string Password)
    : ICommand<OperationResult<User>>;
