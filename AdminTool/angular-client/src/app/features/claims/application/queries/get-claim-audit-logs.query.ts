import { Injectable, inject } from "@angular/core";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class GetClaimAuditLogsQuery {
  private readonly claimRepository = inject<ClaimRepository>(CLAIM_REPOSITORY);


  constructor() {}

  execute(claimId: string) {
    return this.claimRepository.getAuditLogs(claimId);
  }
}
