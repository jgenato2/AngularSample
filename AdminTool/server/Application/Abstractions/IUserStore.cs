using Server.Application.Models;
using Server.Domain.Common;
using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IUserStore
{
    void SeedAdmin(string name, string email, string role, string password);
    IEnumerable<User> List();
    User? FindByEmail(string email);
    User? FindById(string id);
    StoreResult CreateUser(string name, string email, string role, string password);
    StoreResult UpdateUser(string id, UpdateUserModel updates, bool allowRole);
    StoreResult DeleteUser(string id);
}
