import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";

@Injectable({ providedIn: "root" })
export class DeleteClaimCommand {
  constructor(private readonly claimsService: ClaimsService) {}

  execute(claimId: string) {
    return this.claimsService.remove(claimId).pipe(map((response) => !!response.ok));
  }
}
