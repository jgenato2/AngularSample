using Server.Domain.Entities;

namespace Server.Domain.Common;

public record StoreResult(bool Success, User? User, string? Error)
{
    public static StoreResult Ok(User user) => new(true, user, null);
    public static StoreResult Fail(string error) => new(false, null, error);
}
