using Server.Application.Models;

namespace Server.Application.Abstractions;

public interface IAuthApplicationService
{
    OperationResult<AuthPayload> Register(string? name, string? email, string? password);
    OperationResult<AuthPayload> Login(string? email, string? password);
}
