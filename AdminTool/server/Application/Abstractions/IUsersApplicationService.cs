using Server.Application.Models;
using Server.Domain.Entities;
using Server.Presentation.Auditing;

namespace Server.Application.Abstractions;

public interface IUsersApplicationService
{
    IEnumerable<User> List();
    IEnumerable<AuditLogEntry> GetAllAuditLogs();
    OperationResult<User> Create(string? name, string? email, string? role, string? password, string actor);
    OperationResult<User> GetById(string id);
    OperationResult<IEnumerable<AuditLogEntry>> GetAuditLogs(string id);
    OperationResult<User> Update(string id, UpdateUserModel updates, bool allowRole, string actor);
    OperationResult<bool> Delete(string id, string actor);
}
