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

        SeedDemoUsers();
    }

    private void SeedDemoUsers()
    {
        if (_users.Count > 1)
        {
            return;
        }

        var firstNames = new[]
        {
            "Ariana", "Marco", "Bianca", "Diego", "Nadine", "Paolo", "Trisha", "Vincent", "Kara", "Julian",
            "Monica", "Rafael", "Nicole", "Andre", "Camille", "Jericho", "Daphne", "Gabriel", "Selena", "Nathan",
        };
        var lastNames = new[]
        {
            "Reyes", "Mendoza", "Delacruz", "Bautista", "Navarro", "Valdez", "Torres", "Ramos", "Castillo", "Santiago",
            "Garcia", "Flores", "Hernandez", "Ortiz", "Domingo", "Pineda", "Aquino", "Mercado", "Velasco", "Cabrera",
        };
        var domains = new[] { "healthops.local", "carehub.local", "medinet.local", "insuregrid.local" };
        var usedEmails = new HashSet<string>(_users.Select(user => user.Email), StringComparer.OrdinalIgnoreCase);
        var usedNames = new HashSet<string>(_users.Select(user => user.Name), StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        for (var i = 2; i <= 180; i++)
        {
            var index = i - 2;
            var first = firstNames[index % firstNames.Length];
            var last = lastNames[(index / firstNames.Length) % lastNames.Length];
            var baseName = $"{first} {last}";
            var name = baseName;
            if (!usedNames.Add(name))
            {
                var suffix = 2;
                while (true)
                {
                    var candidate = $"{baseName} {suffix}";
                    if (usedNames.Add(candidate))
                    {
                        name = candidate;
                        break;
                    }

                    suffix++;
                }
            }

            var alias = $"{first}.{last}".ToLowerInvariant();
            var domain = domains[index % domains.Length];
            var email = $"{alias}@{domain}";
            if (!usedEmails.Add(email))
            {
                email = $"{alias}{i:000}@{domain}";
                usedEmails.Add(email);
            }

            var createdAt = now
                .AddDays(-(index % 330))
                .AddHours(-((index * 3) % 24))
                .AddMinutes(-((index * 11) % 60));
            var updatedAt = createdAt.AddDays(index % 17).AddMinutes((index * 7) % 60);

            _users.Add(new User
            {
                Id = _nextId.ToString(),
                Name = name,
                Email = email,
                Role = i % 23 == 0 || i % 41 == 0 ? "admin" : "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
            });

            _nextId++;
        }
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
