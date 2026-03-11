using Server.Application.Models;
using Server.Presentation.Auditing;
using Server.Presentation.Contracts;

namespace Server.Application.Abstractions;

public interface IHealthInsuranceApplicationService : IAppInitializer
{
    IEnumerable<HealthInsurancePlanResponse> ListPlans(string actor);
    Task<OperationResult<HealthInsurancePlanResponse>> GetByPolicyId(string policyId, string actor);
    Task<OperationResult<HealthInsuranceFinancialAnalyticsResponse>> GetFinancialAnalytics(string policyId, string actor);
    Task<OperationResult<IEnumerable<AuditLogEntry>>> GetAuditLogs(string policyId);
    IEnumerable<AuditLogEntry> GetListAccessAuditLogs();
    IEnumerable<AuditLogEntry> GetAllAuditLogs();
    InsuranceStatusWorkflowModel GetStatusWorkflow();
    Task<OperationResult<HealthInsurancePlanResponse>> Create(CreateHealthInsurancePlanRequest request, string actor);
    Task<OperationResult<HealthInsurancePlanResponse>> Update(string policyId, UpdateHealthInsurancePlanRequest request, string actor);
    Task<OperationResult<bool>> Delete(string policyId, string actor);
}
