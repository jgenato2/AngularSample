import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";

@Injectable({ providedIn: "root" })
export class DeleteInsurancePlanCommand {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute(policyId: string) {
    return this.insuranceService.remove(policyId).pipe(map((response) => !!response.ok));
  }
}
