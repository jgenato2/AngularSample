import { Inject, Injectable } from "@angular/core";
import { UpdateClaimPayload } from "../../domain/claim.models";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class UpdateClaimCommand {
  constructor(@Inject(CLAIM_REPOSITORY) private readonly claimRepository: ClaimRepository) {}

  execute(claimId: string, payload: UpdateClaimPayload) {
    return this.claimRepository.update(claimId, payload);
  }
}
