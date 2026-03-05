import { InjectionToken } from "@angular/core";
import { Observable } from "rxjs";
import { ClaimAuditLogItem, ClaimItem, ClaimStatusWorkflowResponse, CreateClaimPayload, UpdateClaimPayload } from "./claim.models";

export interface ClaimRepository {
  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>): Observable<ClaimItem[]>;
  getById(claimId: string): Observable<ClaimItem>;
  getStatusWorkflow(): Observable<ClaimStatusWorkflowResponse>;
  getAuditLogs(claimId: string): Observable<ClaimAuditLogItem[]>;
  getListAccessAuditLogs(): Observable<ClaimAuditLogItem[]>;
  create(payload: CreateClaimPayload): Observable<ClaimItem>;
  update(claimId: string, payload: UpdateClaimPayload): Observable<ClaimItem>;
  remove(claimId: string): Observable<boolean>;
}

export const CLAIM_REPOSITORY = new InjectionToken<ClaimRepository>("CLAIM_REPOSITORY");
