import { Injectable } from "@angular/core";
import { CreateInsurancePlanCommand } from "./commands/create-insurance-plan.command";
import { DeleteInsurancePlanCommand } from "./commands/delete-insurance-plan.command";
import { UpdateInsurancePlanCommand } from "./commands/update-insurance-plan.command";
import { GetInsuranceAuditLogsQuery } from "./queries/get-insurance-audit-logs.query";
import { GetInsuranceFinancialAnalyticsQuery } from "./queries/get-insurance-financial-analytics.query";
import { GetInsuranceListAccessAuditLogsQuery } from "./queries/get-insurance-list-access-audit-logs.query";
import { GetInsurancePlanByPolicyIdQuery } from "./queries/get-insurance-plan-by-policy-id.query";
import { GetInsuranceStatusWorkflowQuery } from "./queries/get-insurance-status-workflow.query";
import { ListInsurancePlansQuery } from "./queries/list-insurance-plans.query";
import {
  CreateInsurancePlanPayload,
  UpdateInsurancePlanPayload,
} from "../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class InsuranceFacade {
  constructor(
    private readonly listInsurancePlansQuery: ListInsurancePlansQuery,
    private readonly getInsurancePlanByPolicyIdQuery: GetInsurancePlanByPolicyIdQuery,
    private readonly getInsuranceFinancialAnalyticsQuery: GetInsuranceFinancialAnalyticsQuery,
    private readonly getInsuranceAuditLogsQuery: GetInsuranceAuditLogsQuery,
    private readonly getInsuranceListAccessAuditLogsQuery: GetInsuranceListAccessAuditLogsQuery,
    private readonly getInsuranceStatusWorkflowQuery: GetInsuranceStatusWorkflowQuery,
    private readonly createInsurancePlanCommand: CreateInsurancePlanCommand,
    private readonly updateInsurancePlanCommand: UpdateInsurancePlanCommand,
    private readonly deleteInsurancePlanCommand: DeleteInsurancePlanCommand
  ) {}

  listPlans(sort?: Array<{ field: string; direction: "asc" | "desc" }>) {
    return this.listInsurancePlansQuery.execute(sort);
  }

  getByPolicyId(policyId: string) {
    return this.getInsurancePlanByPolicyIdQuery.execute(policyId);
  }

  getFinancialAnalytics(policyId: string) {
    return this.getInsuranceFinancialAnalyticsQuery.execute(policyId);
  }

  getAuditLogs(policyId: string) {
    return this.getInsuranceAuditLogsQuery.execute(policyId);
  }

  getListAccessAuditLogs() {
    return this.getInsuranceListAccessAuditLogsQuery.execute();
  }

  getStatusWorkflow() {
    return this.getInsuranceStatusWorkflowQuery.execute();
  }

  create(payload: CreateInsurancePlanPayload) {
    return this.createInsurancePlanCommand.execute(payload);
  }

  update(policyId: string, payload: UpdateInsurancePlanPayload) {
    return this.updateInsurancePlanCommand.execute(policyId, payload);
  }

  remove(policyId: string) {
    return this.deleteInsurancePlanCommand.execute(policyId);
  }
}
