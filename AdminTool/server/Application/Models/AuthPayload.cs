using Server.Domain.Entities;

namespace Server.Application.Models;

public sealed class AuthPayload
{
    public string Token { get; init; } = string.Empty;
    public User User { get; init; } = null!;
}
