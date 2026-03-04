using Server.Presentation.Auditing;
using Server.Presentation.Contracts;

namespace Server.Application.Abstractions;

public interface IHealthInsuranceAuditService
{
    void EnsureSeeded();
    void AddListRead(string actor);
    void AddPlanRead(string policyId, string actor);
    void AddFinancialAnalyticsRead(string policyId, string actor);
    void AddPlanCreated(HealthInsurancePlanResponse item, string actor);
    void AddPlanDeleted(HealthInsurancePlanResponse item, string actor);
    void AddChangeAuditLogs(HealthInsurancePlanResponse current, HealthInsurancePlanResponse updated, string actor);
    IEnumerable<AuditLogEntry> GetAuditLogs(string policyId);
    IEnumerable<AuditLogEntry> GetListAccessAuditLogs();
    IEnumerable<AuditLogEntry> GetAllAuditLogs();
}
