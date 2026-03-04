using Server.Application.Abstractions;
using Server.Domain.Entities;

namespace Server.Application.Features.Users.Queries;

public sealed record ListUsersQuery(string Actor) : IQuery<IEnumerable<User>>;
