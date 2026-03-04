using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;
using Server.Presentation.Auditing;

namespace Server.Application.Services;

public sealed class UsersApplicationService(IUserStore userStore) : IUsersApplicationService
{
    private const string AuditScope = "users";

    public IEnumerable<User> List() => userStore.List();

    public IEnumerable<AuditLogEntry> GetAllAuditLogs()
        => AuditLogStore.Query(AuditScope);

    public OperationResult<User> Create(string? name, string? email, string? role, string? password, string actor)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(role) ||
            string.IsNullOrWhiteSpace(password))
        {
            return OperationResult<User>.Fail("Name, email, role, and password required.", ErrorType.Validation);
        }

        var result = userStore.CreateUser(name, email, role, password);
        if (!result.Success)
        {
            return OperationResult<User>.Fail(result.Error ?? "Failed to create user.", ErrorType.Conflict);
        }

        var createdUser = result.User!;
        AuditLogStore.Add(AuditScope, createdUser.Id, "Created", "User", null, $"{createdUser.Name} ({createdUser.Email})", actor);
        AuditLogStore.Add(AuditScope, createdUser.Id, "Updated", "Role", null, createdUser.Role, actor);

        return OperationResult<User>.Ok(createdUser);
    }

    public OperationResult<User> GetById(string id)
    {
        var user = userStore.FindById(id);
        if (user is null)
        {
            return OperationResult<User>.Fail("User not found.", ErrorType.NotFound);
        }

        return OperationResult<User>.Ok(user);
    }

    public OperationResult<IEnumerable<AuditLogEntry>> GetAuditLogs(string id)
    {
        var user = userStore.FindById(id);
        if (user is null)
        {
            return OperationResult<IEnumerable<AuditLogEntry>>.Fail("User not found.", ErrorType.NotFound);
        }

        return OperationResult<IEnumerable<AuditLogEntry>>.Ok(AuditLogStore.Query(AuditScope, id));
    }

    public OperationResult<User> Update(string id, UpdateUserModel updates, bool allowRole, string actor)
    {
        var current = userStore.FindById(id);
        if (current is null)
        {
            return OperationResult<User>.Fail("User not found.", ErrorType.NotFound);
        }

        var previousName = current.Name;
        var previousEmail = current.Email;
        var previousRole = current.Role;
        var hadPasswordUpdate = !string.IsNullOrWhiteSpace(updates.Password);

        var result = userStore.UpdateUser(id, updates, allowRole);
        if (!result.Success)
        {
            var errorType = result.Error == "User not found." ? ErrorType.NotFound : ErrorType.Conflict;
            return OperationResult<User>.Fail(result.Error ?? "Failed to update user.", errorType);
        }

        var updatedUser = result.User!;

        if (!string.Equals(previousName, updatedUser.Name, StringComparison.Ordinal))
        {
            AuditLogStore.Add(AuditScope, updatedUser.Id, "Updated", "Name", previousName, updatedUser.Name, actor);
        }

        if (!string.Equals(previousEmail, updatedUser.Email, StringComparison.OrdinalIgnoreCase))
        {
            AuditLogStore.Add(AuditScope, updatedUser.Id, "Updated", "Email", previousEmail, updatedUser.Email, actor);
        }

        if (!string.Equals(previousRole, updatedUser.Role, StringComparison.Ordinal))
        {
            AuditLogStore.Add(AuditScope, updatedUser.Id, "Updated", "Role", previousRole, updatedUser.Role, actor);
        }

        if (hadPasswordUpdate)
        {
            AuditLogStore.Add(AuditScope, updatedUser.Id, "Updated", "Password", null, "Changed", actor);
        }

        return OperationResult<User>.Ok(updatedUser);
    }

    public OperationResult<bool> Delete(string id, string actor)
    {
        var existingUser = userStore.FindById(id);
        if (existingUser is null)
        {
            return OperationResult<bool>.Fail("User not found.", ErrorType.NotFound);
        }

        var result = userStore.DeleteUser(id);
        if (!result.Success)
        {
            return OperationResult<bool>.Fail(result.Error ?? "Failed to delete user.", ErrorType.NotFound);
        }

        AuditLogStore.Add(AuditScope, existingUser.Id, "Deleted", "User", $"{existingUser.Name} ({existingUser.Email})", null, actor);

        return OperationResult<bool>.Ok(true);
    }
}
