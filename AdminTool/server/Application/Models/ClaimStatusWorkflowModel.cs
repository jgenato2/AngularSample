namespace Server.Application.Models;

public sealed class ClaimStatusWorkflowModel
{
    public IReadOnlyList<string> CreateStatuses { get; init; } = [];
    public IReadOnlyList<ClaimStatusWorkflowItemModel> Workflow { get; init; } = [];
}