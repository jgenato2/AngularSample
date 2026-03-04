using Server.Application.Abstractions;
using Server.Application.Models;
using Server.Domain.Entities;

namespace Server.Application.Services;

public sealed class ClaimsApplicationService(
    IClaimsStore claimsStore,
    IClaimsSeedService claimsSeedService,
    IClaimWorkflowService workflowService,
    IClaimValidationService validationService,
    IClaimAuditService auditService) : IClaimsApplicationService
{
    private static readonly object InitializationLock = new();
    private static bool Initialized;

    public void Initialize() => EnsureInitialized();

    public IEnumerable<Claim> List(string actor)
    {
        auditService.AddListRead(actor);
        return claimsStore.List().OrderByDescending(claim => claim.ServiceDate).ThenBy(claim => claim.ClaimId).ToList();
    }

    public IEnumerable<ClaimAuditLogEntry> GetListAccessAuditLogs()
    {
        return auditService.GetListAccessAuditLogs();
    }

    public IEnumerable<ClaimAuditLogEntry> GetAllAuditLogs()
    {
        return auditService.GetAllAuditLogs();
    }

    public OperationResult<Claim> GetById(string claimId, string actor)
    {
        var item = claimsStore.FindById(claimId);
        if (item is null)
        {
            return OperationResult<Claim>.Fail("Claim not found.", ErrorType.NotFound);
        }

        auditService.AddClaimRead(item.ClaimId, actor);
        return OperationResult<Claim>.Ok(item);
    }

    public OperationResult<IEnumerable<ClaimAuditLogEntry>> GetAuditLogs(string claimId)
    {
        var exists = claimsStore.FindById(claimId) is not null;
        if (!exists)
        {
            return OperationResult<IEnumerable<ClaimAuditLogEntry>>.Fail("Claim not found.", ErrorType.NotFound);
        }

        return OperationResult<IEnumerable<ClaimAuditLogEntry>>.Ok(auditService.GetAuditLogs(claimId));
    }

    public ClaimStatusWorkflowModel GetStatusWorkflow() => workflowService.GetStatusWorkflow();

    public OperationResult<Claim> Create(Claim claim, string actor)
    {
        var validation = validationService.ValidateCreate(claim);
        if (!validation.Success || validation.Value is null)
        {
            return OperationResult<Claim>.Fail(validation.Error ?? "Validation failed.", validation.ErrorType ?? ErrorType.Validation);
        }

        claimsStore.Add(validation.Value);
        auditService.AddClaimCreated(validation.Value, actor);
        return OperationResult<Claim>.Ok(validation.Value);
    }

    public OperationResult<Claim> Update(string claimId, ClaimUpdateModel updates, string actor)
    {
        var current = claimsStore.FindById(claimId);
        if (current is null)
        {
            return OperationResult<Claim>.Fail("Claim not found.", ErrorType.NotFound);
        }

        var validation = validationService.ValidateAndBuildUpdated(current, updates);
        if (!validation.Success || validation.Value is null)
        {
            return OperationResult<Claim>.Fail(validation.Error ?? "Validation failed.", validation.ErrorType ?? ErrorType.Validation);
        }

        claimsStore.Update(validation.Value);
        auditService.AddChangeAuditLogs(current, validation.Value, actor);
        return OperationResult<Claim>.Ok(validation.Value);
    }

    public OperationResult<bool> Delete(string claimId, string actor)
    {
        var deleted = claimsStore.Delete(claimId, out var removed);
        if (!deleted || removed is null)
        {
            return OperationResult<bool>.Fail("Claim not found.", ErrorType.NotFound);
        }

        auditService.AddClaimDeleted(removed, actor);
        return OperationResult<bool>.Ok(true);
    }

    private void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        lock (InitializationLock)
        {
            if (Initialized)
            {
                return;
            }

            claimsSeedService.EnsureSeeded();
            auditService.EnsureSeeded();
            Initialized = true;
        }
    }
}