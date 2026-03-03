namespace Server.Presentation.Contracts;

public record ClaimResponse(
    string ClaimId,
    string PolicyId,
    string MemberName,
    string Provider,
    string ClaimType,
    string ServiceCategory,
    string DiagnosisCode,
    DateTime SubmittedAt,
    DateTime ServiceDate,
    decimal ClaimAmount,
    string Status,
    string? Notes);
