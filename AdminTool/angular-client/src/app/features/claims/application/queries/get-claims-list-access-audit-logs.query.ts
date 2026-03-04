import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";
import { ClaimAuditLogItem } from "../../domain/claim.models";

@Injectable({ providedIn: "root" })
export class GetClaimsListAccessAuditLogsQuery {
  constructor(private readonly claimsService: ClaimsService) {}

  execute() {
    return this.claimsService.getListAccessAuditLogs().pipe(map((response) => (response.items ?? []) as ClaimAuditLogItem[]));
  }
}
