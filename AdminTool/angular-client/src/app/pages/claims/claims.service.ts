import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { ClaimAuditLogItem, ClaimItem, ClaimStatusWorkflowResponse, CreateClaimPayload, UpdateClaimPayload } from "../../features/claims/domain/claim.models";

interface ClaimListResponse {
  items: ClaimItem[];
}

interface ClaimItemResponse {
  item: ClaimItem;
}

interface ClaimAuditLogListResponse {
  items: ClaimAuditLogItem[];
}

@Injectable({ providedIn: "root" })
export class ClaimsService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = "/api/claims";


  constructor() {}

  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>) {
    let params = new HttpParams();

    for (const item of sort ?? []) {
      params = params.append("sort", `${item.field}:${item.direction}`);
    }

    return this.http.get<ClaimListResponse>(this.baseUrl, { params });
  }

  getById(claimId: string) {
    return this.http.get<ClaimItemResponse>(`${this.baseUrl}/${claimId}`);
  }

  getStatusWorkflow() {
    return this.http.get<ClaimStatusWorkflowResponse>(`${this.baseUrl}/status-workflow`);
  }

  getAuditLogs(claimId: string) {
    return this.http.get<ClaimAuditLogListResponse>(`${this.baseUrl}/${claimId}/audit-logs`);
  }

  getListAccessAuditLogs() {
    return this.http.get<ClaimAuditLogListResponse>(`${this.baseUrl}/audit-logs/list-access`);
  }

  create(payload: CreateClaimPayload) {
    return this.http.post<ClaimItemResponse>(this.baseUrl, payload);
  }

  update(claimId: string, payload: UpdateClaimPayload) {
    return this.http.put<ClaimItemResponse>(`${this.baseUrl}/${claimId}`, payload);
  }

  remove(claimId: string) {
    return this.http.delete<{ ok: boolean }>(`${this.baseUrl}/${claimId}`);
  }
}

