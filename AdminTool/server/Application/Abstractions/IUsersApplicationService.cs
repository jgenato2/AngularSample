using Server.Application.Models;
using Server.Domain.Entities;
using Server.Presentation.Auditing;

namespace Server.Application.Abstractions;

public interface IUsersApplicationService
{
    IEnumerable<User> List();
    IEnumerable<AuditLogEntry> GetAllAuditLogs();
    Task<OperationResult<User>> Create(string? name, string? email, string? role, string? password, string actor);
    Task<OperationResult<User>> GetById(string id);
    Task<OperationResult<IEnumerable<AuditLogEntry>>> GetAuditLogs(string id);
    Task<OperationResult<User>> Update(string id, UpdateUserModel updates, bool allowRole, string actor);
    Task<OperationResult<bool>> Delete(string id, string actor);
}
