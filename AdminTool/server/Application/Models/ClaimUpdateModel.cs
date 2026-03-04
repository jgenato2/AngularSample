namespace Server.Application.Models;

public sealed class ClaimUpdateModel
{
    public string? PolicyId { get; init; }
    public string? MemberName { get; init; }
    public string? Provider { get; init; }
    public string? ClaimType { get; init; }
    public string? ServiceCategory { get; init; }
    public string? DiagnosisCode { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? ServiceDate { get; init; }
    public decimal? ClaimAmount { get; init; }
    public string? Status { get; init; }
    public string? Notes { get; init; }
}