import { Injectable, inject } from "@angular/core";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class GetClaimStatusWorkflowQuery {
  private readonly claimRepository = inject<ClaimRepository>(CLAIM_REPOSITORY);


  constructor() {}

  execute() {
    return this.claimRepository.getStatusWorkflow();
  }
}
