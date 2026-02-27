using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Common;
using Server.Domain.Entities;

namespace Server.Infrastructure.Persistence;

public sealed class UserStore : IUserStore
{
    private readonly List<User> _users = new();
    private int _nextId = 2;

    public void SeedAdmin(string name, string email, string role, string password)
    {
        if (FindByEmail(email) is not null)
        {
            return;
        }

        _users.Add(new User
        {
            Id = "1",
            Name = name,
            Email = email,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
    }

    public IEnumerable<User> List() => _users;

    public User? FindByEmail(string email) =>
        _users.FirstOrDefault(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public User? FindById(string id) => _users.FirstOrDefault(user => user.Id == id);

    public StoreResult CreateUser(string name, string email, string role, string password)
    {
        if (FindByEmail(email) != null)
        {
            return StoreResult.Fail("Email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = _nextId.ToString(),
            Name = name,
            Email = email,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _nextId++;
        _users.Add(user);
        return StoreResult.Ok(user);
    }

    public StoreResult UpdateUser(string id, UpdateUserModel updates, bool allowRole)
    {
        var user = FindById(id);
        if (user is null)
        {
            return StoreResult.Fail("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(updates.Email) &&
            !updates.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) &&
            FindByEmail(updates.Email) != null)
        {
            return StoreResult.Fail("Email already exists.");
        }

        if (!string.IsNullOrWhiteSpace(updates.Name))
        {
            user.Name = updates.Name;
        }

        if (!string.IsNullOrWhiteSpace(updates.Email))
        {
            user.Email = updates.Email;
        }

        if (allowRole && !string.IsNullOrWhiteSpace(updates.Role))
        {
            user.Role = updates.Role;
        }

        if (!string.IsNullOrWhiteSpace(updates.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updates.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;
        return StoreResult.Ok(user);
    }

    public StoreResult DeleteUser(string id)
    {
        var user = FindById(id);
        if (user is null)
        {
            return StoreResult.Fail("User not found.");
        }

        _users.Remove(user);
        return StoreResult.Ok(user);
    }
}
