using Server.Application.Models;

namespace Server.Application.Abstractions;

public interface IAuthApplicationService
{
    Task<OperationResult<AuthPayload>> Register(string? name, string? email, string? password);
    Task<OperationResult<AuthPayload>> Login(string? email, string? password);
}
