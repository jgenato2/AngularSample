using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Abstractions;

public interface IClaimsApplicationService : IAppInitializer
{
    IEnumerable<Claim> List(string actor);
    IEnumerable<ClaimAuditLogEntry> GetListAccessAuditLogs();
    IEnumerable<ClaimAuditLogEntry> GetAllAuditLogs();
    Task<OperationResult<Claim>> GetById(string claimId, string actor);
    Task<OperationResult<IEnumerable<ClaimAuditLogEntry>>> GetAuditLogs(string claimId);
    ClaimStatusWorkflowModel GetStatusWorkflow();
    Task<OperationResult<Claim>> Create(Claim claim, string actor);
    Task<OperationResult<Claim>> Update(string claimId, ClaimUpdateModel updates, string actor);
    Task<OperationResult<bool>> Delete(string claimId, string actor);
}