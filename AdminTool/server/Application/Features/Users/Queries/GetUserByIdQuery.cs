using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Queries;

public sealed record GetUserByIdQuery(string Id) : IQuery<OperationResult<User>>;
