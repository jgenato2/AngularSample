import { Inject, Injectable } from "@angular/core";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class DeleteClaimCommand {
  constructor(@Inject(CLAIM_REPOSITORY) private readonly claimRepository: ClaimRepository) {}

  execute(claimId: string) {
    return this.claimRepository.remove(claimId);
  }
}
