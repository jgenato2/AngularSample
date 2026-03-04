import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";
import { ClaimItem } from "../../domain/claim.models";

@Injectable({ providedIn: "root" })
export class GetClaimByIdQuery {
  constructor(private readonly claimsService: ClaimsService) {}

  execute(claimId: string) {
    return this.claimsService.getById(claimId).pipe(map((response) => response.item as ClaimItem));
  }
}
