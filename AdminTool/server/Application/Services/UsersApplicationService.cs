using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Services;

public sealed class UsersApplicationService(IUserStore userStore) : IUsersApplicationService
{
    public IEnumerable<User> List() => userStore.List();

    public OperationResult<User> Create(string? name, string? email, string? role, string? password)
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

        return OperationResult<User>.Ok(result.User!);
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

    public OperationResult<User> Update(string id, UpdateUserModel updates, bool allowRole)
    {
        var result = userStore.UpdateUser(id, updates, allowRole);
        if (!result.Success)
        {
            var errorType = result.Error == "User not found." ? ErrorType.NotFound : ErrorType.Conflict;
            return OperationResult<User>.Fail(result.Error ?? "Failed to update user.", errorType);
        }

        return OperationResult<User>.Ok(result.User!);
    }

    public OperationResult<bool> Delete(string id)
    {
        var result = userStore.DeleteUser(id);
        if (!result.Success)
        {
            return OperationResult<bool>.Fail(result.Error ?? "Failed to delete user.", ErrorType.NotFound);
        }

        return OperationResult<bool>.Ok(true);
    }
}
