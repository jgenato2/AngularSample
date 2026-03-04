import { Inject, Injectable } from "@angular/core";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class ListClaimsQuery {
  constructor(@Inject(CLAIM_REPOSITORY) private readonly claimRepository: ClaimRepository) {}

  execute() {
    return this.claimRepository.list();
  }
}
