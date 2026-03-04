using Server.Application.Models;
using Server.Presentation.Auditing;
using Server.Presentation.Contracts;

namespace Server.Application.Abstractions;

public interface IHealthInsuranceApplicationService : IAppInitializer
{
    IEnumerable<HealthInsurancePlanResponse> ListPlans(string actor);
    OperationResult<HealthInsurancePlanResponse> GetByPolicyId(string policyId, string actor);
    OperationResult<HealthInsuranceFinancialAnalyticsResponse> GetFinancialAnalytics(string policyId, string actor);
    OperationResult<IEnumerable<AuditLogEntry>> GetAuditLogs(string policyId);
    IEnumerable<AuditLogEntry> GetListAccessAuditLogs();
    IEnumerable<AuditLogEntry> GetAllAuditLogs();
    InsuranceStatusWorkflowModel GetStatusWorkflow();
    OperationResult<HealthInsurancePlanResponse> Create(CreateHealthInsurancePlanRequest request, string actor);
    OperationResult<HealthInsurancePlanResponse> Update(string policyId, UpdateHealthInsurancePlanRequest request, string actor);
    OperationResult<bool> Delete(string policyId, string actor);
}
