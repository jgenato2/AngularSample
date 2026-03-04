using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Commands;

public sealed record UpdateUserCommand(string Id, UpdateUserModel Updates, bool AllowRole)
    : ICommand<OperationResult<User>>;
