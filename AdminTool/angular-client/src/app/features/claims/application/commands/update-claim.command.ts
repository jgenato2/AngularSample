import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";
import { ClaimItem, UpdateClaimPayload } from "../../domain/claim.models";

@Injectable({ providedIn: "root" })
export class UpdateClaimCommand {
  constructor(private readonly claimsService: ClaimsService) {}

  execute(claimId: string, payload: UpdateClaimPayload) {
    return this.claimsService.update(claimId, payload).pipe(map((response) => response.item as ClaimItem));
  }
}
