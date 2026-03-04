using Server.Application.Abstractions;
using Server.Application.Models;

namespace Server.Application.Services;

public sealed class ClaimWorkflowService : IClaimWorkflowService
{
    private static readonly StringComparer StatusComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly IReadOnlyDictionary<string, string[]> StatusWorkflow =
        new Dictionary<string, string[]>(StatusComparer)
        {
            ["Submitted"] = ["Under Review", "Rejected"],
            ["Under Review"] = ["Approved", "Rejected"],
            ["Approved"] = ["Approved"],
            ["Rejected"] = ["Submitted"],
        };

    private static readonly List<string> InitialStatuses = ["Submitted"];

    public IReadOnlyList<string> AllowedInitialStatuses => InitialStatuses;

    public ClaimStatusWorkflowModel GetStatusWorkflow()
    {
        var workflow = StatusWorkflow
            .Select(entry => new ClaimStatusWorkflowItemModel
            {
                Status = entry.Key,
                Next = entry.Value,
            })
            .ToList();

        return new ClaimStatusWorkflowModel
        {
            CreateStatuses = InitialStatuses.OrderBy(status => status).ToArray(),
            Workflow = workflow,
        };
    }

    public string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var trimmed = status.Trim();
        return StatusWorkflow.Keys.FirstOrDefault(value => value.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
    }

    public bool CanTransition(string currentStatus, string nextStatus)
    {
        if (!StatusWorkflow.TryGetValue(currentStatus, out var transitions))
        {
            return false;
        }

        return transitions.Contains(nextStatus, StringComparer.OrdinalIgnoreCase);
    }
}
