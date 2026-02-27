using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IUsersApplicationService
{
    IEnumerable<User> List();
    OperationResult<User> Create(string? name, string? email, string? role, string? password);
    OperationResult<User> GetById(string id);
    OperationResult<User> Update(string id, UpdateUserModel updates, bool allowRole);
    OperationResult<bool> Delete(string id);
}
