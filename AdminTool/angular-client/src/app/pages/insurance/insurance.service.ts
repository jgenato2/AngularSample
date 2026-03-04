import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import {
  InsuranceAuditLogItem,
  CreateInsurancePlanPayload,
  InsuranceFinancialAnalyticsItem,
  InsurancePlanItem,
  InsuranceStatusWorkflowResponse,
  UpdateInsurancePlanPayload,
} from "../../features/insurance/domain/insurance.models";

interface PlanListResponse {
  items: InsurancePlanItem[];
}

interface PlanItemResponse {
  item: InsurancePlanItem;
}

interface FinancialAnalyticsResponse {
  item: InsuranceFinancialAnalyticsItem;
}

interface AuditLogListResponse {
  items: InsuranceAuditLogItem[];
}

@Injectable({ providedIn: "root" })
export class InsuranceService {
  private readonly baseUrl = "/api/health-insurance";

  constructor(private readonly http: HttpClient) {}

  listPlans() {
    return this.http.get<PlanListResponse>(`${this.baseUrl}/plans`);
  }

  getByPolicyId(policyId: string) {
    return this.http.get<PlanItemResponse>(`${this.baseUrl}/plans/${policyId}`);
  }

  getFinancialAnalytics(policyId: string) {
    return this.http.get<FinancialAnalyticsResponse>(`${this.baseUrl}/plans/${policyId}/financial-analytics`);
  }

  getAuditLogs(policyId: string) {
    return this.http.get<AuditLogListResponse>(`${this.baseUrl}/plans/${policyId}/audit-logs`);
  }

  getListAccessAuditLogs() {
    return this.http.get<AuditLogListResponse>(`${this.baseUrl}/audit-logs/list-access`);
  }

  getStatusWorkflow() {
    return this.http.get<InsuranceStatusWorkflowResponse>(`${this.baseUrl}/status-workflow`);
  }

  create(payload: CreateInsurancePlanPayload) {
    return this.http.post<PlanItemResponse>(`${this.baseUrl}/plans`, payload);
  }

  update(policyId: string, payload: UpdateInsurancePlanPayload) {
    return this.http.put<PlanItemResponse>(`${this.baseUrl}/plans/${policyId}`, payload);
  }

  remove(policyId: string) {
    return this.http.delete<{ ok: boolean }>(`${this.baseUrl}/plans/${policyId}`);
  }
}

