import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { ClaimsService } from "../../../../claims/claims.service";

@Injectable({ providedIn: "root" })
export class ListClaimsQuery {
  constructor(private readonly claimsService: ClaimsService) {}

  execute() {
    return this.claimsService.list().pipe(map((response) => response.items ?? []));
  }
}
