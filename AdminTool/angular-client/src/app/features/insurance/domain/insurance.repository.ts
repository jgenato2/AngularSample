import { InjectionToken } from "@angular/core";
import { Observable } from "rxjs";
import {
  CreateInsurancePlanPayload,
  InsuranceAuditLogItem,
  InsuranceFinancialAnalyticsItem,
  InsurancePlanItem,
  InsuranceStatusWorkflowResponse,
  UpdateInsurancePlanPayload,
} from "./insurance.models";

export interface InsuranceRepository {
  listPlans(sort?: Array<{ field: string; direction: "asc" | "desc" }>): Observable<InsurancePlanItem[]>;
  getByPolicyId(policyId: string): Observable<InsurancePlanItem>;
  getFinancialAnalytics(policyId: string): Observable<InsuranceFinancialAnalyticsItem>;
  getAuditLogs(policyId: string): Observable<InsuranceAuditLogItem[]>;
  getListAccessAuditLogs(): Observable<InsuranceAuditLogItem[]>;
  getStatusWorkflow(): Observable<InsuranceStatusWorkflowResponse>;
  create(payload: CreateInsurancePlanPayload): Observable<InsurancePlanItem>;
  update(policyId: string, payload: UpdateInsurancePlanPayload): Observable<InsurancePlanItem>;
  remove(policyId: string): Observable<boolean>;
}

export const INSURANCE_REPOSITORY = new InjectionToken<InsuranceRepository>("INSURANCE_REPOSITORY");
