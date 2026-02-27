using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(User user);
}
