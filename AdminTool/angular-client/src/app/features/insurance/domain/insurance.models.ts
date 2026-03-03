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
  comments?: string | null;
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

export interface InsuranceAuditLogItem {
  id: string;
  policyId: string;
  action: string;
  field: string;
  oldValue?: string | null;
  newValue?: string | null;
  performedBy: string;
  occurredAtUtc: string;
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
  comments?: string | null;
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
  comments?: string | null;
}
