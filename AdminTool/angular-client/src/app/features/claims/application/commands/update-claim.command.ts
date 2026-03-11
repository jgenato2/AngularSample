import { Injectable, inject } from "@angular/core";
import { UpdateClaimPayload } from "../../domain/claim.models";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class UpdateClaimCommand {
  private readonly claimRepository = inject<ClaimRepository>(CLAIM_REPOSITORY);


  constructor() {}

  execute(claimId: string, payload: UpdateClaimPayload) {
    return this.claimRepository.update(claimId, payload);
  }
}
