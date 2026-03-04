using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Services;

public sealed class HealthInsuranceWorkflowService : IHealthInsuranceWorkflowService
{
    private static readonly StringComparer StatusComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly IReadOnlyDictionary<string, string[]> StatusWorkflow =
        new Dictionary<string, string[]>(StatusComparer)
        {
            ["New"] = ["Underwriting", "Cancelled"],
            ["Underwriting"] = ["Pending Activation", "Cancelled"],
            ["Pending Activation"] = ["Active", "Cancelled"],
            ["Active"] = ["Grace Period", "Pending Renewal", "Suspended", "Cancelled", "Expired"],
            ["Grace Period"] = ["Active", "Suspended", "Cancelled", "Expired"],
            ["Pending Renewal"] = ["Renewed", "Expired", "Cancelled"],
            ["Renewed"] = ["Active", "Cancelled"],
            ["Suspended"] = ["Active", "Cancelled", "Expired"],
            ["Cancelled"] = ["New"],
            ["Expired"] = ["New"],
        };

    private static readonly List<string> InitialStatuses = ["New"];

    public IReadOnlyList<string> AllowedInitialStatuses => InitialStatuses;

    public InsuranceStatusWorkflowModel GetStatusWorkflow()
    {
        var workflow = StatusWorkflow
            .OrderBy(item => item.Key)
            .Select(item => new InsuranceStatusWorkflowItemModel
            {
                Status = item.Key,
                Next = item.Value,
            })
            .ToList();

        return new InsuranceStatusWorkflowModel
        {
            CreateStatuses = InitialStatuses.OrderBy(value => value).ToList(),
            Workflow = workflow,
        };
    }

    public string? NormalizeStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (StatusComparer.Equals(trimmed, "Draft"))
        {
            return "New";
        }

        return StatusWorkflow.Keys.FirstOrDefault(status => StatusComparer.Equals(status, trimmed));
    }

    public bool CanTransition(string currentStatus, string nextStatus)
    {
        var normalizedCurrentStatus = NormalizeStatus(currentStatus) ?? currentStatus;
        var normalizedNextStatus = NormalizeStatus(nextStatus) ?? nextStatus;

        if (StatusComparer.Equals(normalizedCurrentStatus, normalizedNextStatus))
        {
            return true;
        }

        if (!StatusWorkflow.TryGetValue(normalizedCurrentStatus, out var allowedNextStatuses))
        {
            return false;
        }

        return allowedNextStatuses.Any(status => StatusComparer.Equals(status, normalizedNextStatus));
    }
}
