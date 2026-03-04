using Server.Application.Models;

namespace Server.Application.Abstractions;

public interface IClaimWorkflowService
{
    IReadOnlyList<string> AllowedInitialStatuses { get; }
    ClaimStatusWorkflowModel GetStatusWorkflow();
    string? NormalizeStatus(string? status);
    bool CanTransition(string currentStatus, string nextStatus);
}
