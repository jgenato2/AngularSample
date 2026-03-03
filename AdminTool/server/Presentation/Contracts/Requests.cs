namespace Server.Presentation.Contracts;

public record LoginRequest(string email, string password);
public record RegisterRequest(string name, string email, string password);
public record CreateUserRequest(string name, string email, string role, string password);
public record UpdateUserRequest(string? name, string? email, string? role, string? password);
public record CreateHealthInsurancePlanRequest(
	string policyId,
	string memberName,
	string provider,
	string planType,
	decimal monthlyPremium,
	decimal deductible,
	decimal outOfPocketMax,
	string status,
	DateTime effectiveDate,
	DateTime renewalDate,
	string? comments);
public record UpdateHealthInsurancePlanRequest(
	string? memberName,
	string? provider,
	string? planType,
	decimal? monthlyPremium,
	decimal? deductible,
	decimal? outOfPocketMax,
	string? status,
	DateTime? effectiveDate,
	DateTime? renewalDate,
	string? comments);
public record CreateClaimRequest(
	string claimId,
	string policyId,
	string memberName,
	string provider,
	string claimType,
	string serviceCategory,
	string diagnosisCode,
	DateTime submittedAt,
	DateTime serviceDate,
	decimal claimAmount,
	string status,
	string? notes);
public record UpdateClaimRequest(
	string? policyId,
	string? memberName,
	string? provider,
	string? claimType,
	string? serviceCategory,
	string? diagnosisCode,
	DateTime? submittedAt,
	DateTime? serviceDate,
	decimal? claimAmount,
	string? status,
	string? notes);
