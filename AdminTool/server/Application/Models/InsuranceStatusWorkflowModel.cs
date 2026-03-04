namespace Server.Application.Models;

public sealed class InsuranceStatusWorkflowModel
{
    public required IReadOnlyList<string> CreateStatuses { get; init; }
    public required IReadOnlyList<InsuranceStatusWorkflowItemModel> Workflow { get; init; }
}

public sealed class InsuranceStatusWorkflowItemModel
{
    public required string Status { get; init; }
    public required IReadOnlyList<string> Next { get; init; }
}
