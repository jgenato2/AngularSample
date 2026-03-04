import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";
import { ClaimItem, CreateClaimPayload } from "../../domain/claim.models";

@Injectable({ providedIn: "root" })
export class CreateClaimCommand {
  constructor(private readonly claimsService: ClaimsService) {}

  execute(payload: CreateClaimPayload) {
    return this.claimsService.create(payload).pipe(map((response) => response.item as ClaimItem));
  }
}
