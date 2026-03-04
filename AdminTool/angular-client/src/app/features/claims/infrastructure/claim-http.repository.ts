import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../pages/claims/claims.service";
import {
  ClaimAuditLogItem,
  ClaimItem,
  ClaimStatusWorkflowResponse,
  CreateClaimPayload,
  UpdateClaimPayload,
} from "../domain/claim.models";
import { ClaimRepository } from "../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class ClaimHttpRepository implements ClaimRepository {
  constructor(private readonly claimsService: ClaimsService) {}

  list() {
    return this.claimsService.list().pipe(map((response) => response.items ?? []));
  }

  getById(claimId: string) {
    return this.claimsService.getById(claimId).pipe(map((response) => response.item as ClaimItem));
  }

  getStatusWorkflow() {
    return this.claimsService.getStatusWorkflow() as ReturnType<ClaimsService["getStatusWorkflow"]>;
  }

  getAuditLogs(claimId: string) {
    return this.claimsService.getAuditLogs(claimId).pipe(map((response) => (response.items ?? []) as ClaimAuditLogItem[]));
  }

  getListAccessAuditLogs() {
    return this.claimsService.getListAccessAuditLogs().pipe(map((response) => (response.items ?? []) as ClaimAuditLogItem[]));
  }

  create(payload: CreateClaimPayload) {
    return this.claimsService.create(payload).pipe(map((response) => response.item as ClaimItem));
  }

  update(claimId: string, payload: UpdateClaimPayload) {
    return this.claimsService.update(claimId, payload).pipe(map((response) => response.item as ClaimItem));
  }

  remove(claimId: string) {
    return this.claimsService.remove(claimId).pipe(map((response) => !!response.ok));
  }
}
