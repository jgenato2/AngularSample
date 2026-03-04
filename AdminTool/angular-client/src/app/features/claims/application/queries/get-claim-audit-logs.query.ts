import { Inject, Injectable } from "@angular/core";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class GetClaimAuditLogsQuery {
  constructor(@Inject(CLAIM_REPOSITORY) private readonly claimRepository: ClaimRepository) {}

  execute(claimId: string) {
    return this.claimRepository.getAuditLogs(claimId);
  }
}
