using Server.Domain.Entities;
using Server.Presentation.Contracts;

namespace Server.Presentation.Mappings;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.Name, user.Email, user.Role, user.CreatedAt, user.UpdatedAt);
}
