import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";
import { InsurancePlanItem } from "../../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class GetInsurancePlanByPolicyIdQuery {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute(policyId: string) {
    return this.insuranceService.getByPolicyId(policyId).pipe(map((response) => response.item as InsurancePlanItem));
  }
}
