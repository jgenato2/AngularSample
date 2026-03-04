import { Inject, Injectable } from "@angular/core";
import { CreateClaimPayload } from "../../domain/claim.models";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class CreateClaimCommand {
  constructor(@Inject(CLAIM_REPOSITORY) private readonly claimRepository: ClaimRepository) {}

  execute(payload: CreateClaimPayload) {
    return this.claimRepository.create(payload);
  }
}
