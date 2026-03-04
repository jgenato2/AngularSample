namespace Server.Domain.Entities;

public sealed class Claim
{
    public string ClaimId { get; init; } = "";
    public string PolicyId { get; set; } = "";
    public string MemberName { get; set; } = "";
    public string Provider { get; set; } = "";
    public string ClaimType { get; set; } = "";
    public string ServiceCategory { get; set; } = "";
    public string DiagnosisCode { get; set; } = "";
    public DateTime SubmittedAt { get; set; }
    public DateTime ServiceDate { get; set; }
    public decimal ClaimAmount { get; set; }
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
}