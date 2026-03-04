import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";
import { ClaimAuditLogItem } from "../../domain/claim.models";

@Injectable({ providedIn: "root" })
export class GetClaimAuditLogsQuery {
  constructor(private readonly claimsService: ClaimsService) {}

  execute(claimId: string) {
    return this.claimsService.getAuditLogs(claimId).pipe(map((response) => (response.items ?? []) as ClaimAuditLogItem[]));
  }
}
