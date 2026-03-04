using Server.Application.Models;

namespace Server.Application.Abstractions;

public interface IHealthInsuranceWorkflowService
{
    IReadOnlyList<string> AllowedInitialStatuses { get; }
    InsuranceStatusWorkflowModel GetStatusWorkflow();
    string? NormalizeStatus(string? value);
    bool CanTransition(string currentStatus, string nextStatus);
}
