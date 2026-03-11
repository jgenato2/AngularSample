import { Injectable, inject } from "@angular/core";
import { CreateClaimPayload } from "../../domain/claim.models";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class CreateClaimCommand {
  private readonly claimRepository = inject<ClaimRepository>(CLAIM_REPOSITORY);


  constructor() {}

  execute(payload: CreateClaimPayload) {
    return this.claimRepository.create(payload);
  }
}
