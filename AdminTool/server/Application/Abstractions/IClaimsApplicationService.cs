using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IClaimsApplicationService
{
    void Initialize();
    IEnumerable<Claim> List(string actor);
    IEnumerable<ClaimAuditLogEntry> GetListAccessAuditLogs();
    OperationResult<Claim> GetById(string claimId, string actor);
    OperationResult<IEnumerable<ClaimAuditLogEntry>> GetAuditLogs(string claimId);
    ClaimStatusWorkflowModel GetStatusWorkflow();
    OperationResult<Claim> Create(Claim claim, string actor);
    OperationResult<Claim> Update(string claimId, ClaimUpdateModel updates, string actor);
    OperationResult<bool> Delete(string claimId, string actor);
}