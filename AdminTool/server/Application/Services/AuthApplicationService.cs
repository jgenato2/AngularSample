using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Services;

public sealed class AuthApplicationService(IUserStore userStore, ITokenService tokenService) : IAuthApplicationService
{
    public Task<OperationResult<AuthPayload>> Register(string? name, string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(OperationResult<AuthPayload>.Fail("Name, email, and password required.", ErrorType.Validation));
        }

        var result = userStore.CreateUser(name, email, "user", password);
        if (!result.Success)
        {
            return Task.FromResult(OperationResult<AuthPayload>.Fail(result.Error ?? "Registration failed.", ErrorType.Conflict));
        }

        var user = result.User!;
        var token = tokenService.CreateToken(user);
        return Task.FromResult(OperationResult<AuthPayload>.Ok(new AuthPayload
        {
            Token = token,
            User = user,
        }));
    }

    public Task<OperationResult<AuthPayload>> Login(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(OperationResult<AuthPayload>.Fail("Email and password required.", ErrorType.Validation));
        }

        var user = userStore.FindByEmail(email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return Task.FromResult(OperationResult<AuthPayload>.Fail("Invalid credentials.", ErrorType.Unauthorized));
        }

        var token = tokenService.CreateToken(user);
        return Task.FromResult(OperationResult<AuthPayload>.Ok(new AuthPayload
        {
            Token = token,
            User = user,
        }));
    }
}
