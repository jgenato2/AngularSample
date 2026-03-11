import { Injectable, inject } from "@angular/core";
import { CLAIM_REPOSITORY, ClaimRepository } from "../../domain/claim.repository";

@Injectable({ providedIn: "root" })
export class ListClaimsQuery {
  private readonly claimRepository = inject<ClaimRepository>(CLAIM_REPOSITORY);


  constructor() {}

  execute(sort?: Array<{ field: string; direction: "asc" | "desc" }>) {
    return this.claimRepository.list(sort);
  }
}
