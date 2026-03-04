namespace Server.Application.Models;

public sealed class ClaimStatusWorkflowItemModel
{
    public string Status { get; init; } = "";
    public IReadOnlyList<string> Next { get; init; } = [];
}