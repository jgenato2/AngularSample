export interface ClaimItem {
  claimId: string;
  policyId: string;
  memberName: string;
  provider: string;
  claimType: string;
  serviceCategory: string;
  diagnosisCode: string;
  submittedAt: string;
  serviceDate: string;
  claimAmount: number;
  status: string;
  notes?: string | null;
}

export interface CreateClaimPayload {
  claimId: string;
  policyId: string;
  memberName: string;
  provider: string;
  claimType: string;
  serviceCategory: string;
  diagnosisCode: string;
  submittedAt: string;
  serviceDate: string;
  claimAmount: number;
  status: string;
  notes?: string;
}

export interface UpdateClaimPayload {
  policyId?: string;
  memberName?: string;
  provider?: string;
  claimType?: string;
  serviceCategory?: string;
  diagnosisCode?: string;
  submittedAt?: string;
  serviceDate?: string;
  claimAmount?: number;
  status?: string;
  notes?: string;
}

export interface ClaimStatusWorkflowItem {
  status: string;
  next: string[];
}

export interface ClaimStatusWorkflowResponse {
  createStatuses: string[];
  workflow: ClaimStatusWorkflowItem[];
}

export interface ClaimAuditLogItem {
  id: string;
  claimId: string;
  action: string;
  field: string;
  oldValue?: string | null;
  newValue?: string | null;
  performedBy: string;
  occurredAtUtc: string;
}
