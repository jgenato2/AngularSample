import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

export interface InsurancePlanItem {
  policyId: string;
  memberName: string;
  provider: string;
  planType: string;
  monthlyPremium: number;
  deductible: number;
  outOfPocketMax: number;
  status: string;
  effectiveDate: string;
  renewalDate: string;
}

export interface InsuranceFinancialAnalyticsItem {
  annualPremium: number;
  deductibleRatio: number;
  deductibleLeverageIndex: number;
  projectedClaimsCost: number;
  projectedLossRatio: number;
  premiumAdequacyRatio: number;
  trendAdjustedClaimsCost: number;
  volatilityBuffer: number;
  capitalAtRisk95: number;
  tailRiskRatio: number;
  reserveRequirement: number;
  combinedCapitalNeed: number;
  solvencyMargin: number;
  stressScenarioCost: number;
  stressImpact: number;
  stressScenarioMargin: number;
  stabilityIndex: number;
  riskScore: number;
  riskBand: string;
}

export interface InsuranceStatusWorkflowItem {
  status: string;
  next: string[];
}

export interface InsuranceStatusWorkflowResponse {
  createStatuses: string[];
  workflow: InsuranceStatusWorkflowItem[];
}

export interface CreateInsurancePlanPayload {
  policyId: string;
  memberName: string;
  provider: string;
  planType: string;
  monthlyPremium: number;
  deductible: number;
  outOfPocketMax: number;
  status: string;
  effectiveDate: string;
  renewalDate: string;
}

export interface UpdateInsurancePlanPayload {
  memberName?: string;
  provider?: string;
  planType?: string;
  monthlyPremium?: number;
  deductible?: number;
  outOfPocketMax?: number;
  status?: string;
  effectiveDate?: string;
  renewalDate?: string;
}

interface PlanListResponse {
  items: InsurancePlanItem[];
}

interface PlanItemResponse {
  item: InsurancePlanItem;
}

interface FinancialAnalyticsResponse {
  item: InsuranceFinancialAnalyticsItem;
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
