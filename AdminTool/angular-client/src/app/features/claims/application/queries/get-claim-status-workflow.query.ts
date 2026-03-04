import { Injectable } from "@angular/core";
import { ClaimsService } from "../../../../claims/claims.service";

@Injectable({ providedIn: "root" })
export class GetClaimStatusWorkflowQuery {
  constructor(private readonly claimsService: ClaimsService) {}

  execute() {
    return this.claimsService.getStatusWorkflow() as ReturnType<ClaimsService["getStatusWorkflow"]>;
  }
}
