import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";
import { InsurancePlanItem, UpdateInsurancePlanPayload } from "../../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class UpdateInsurancePlanCommand {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute(policyId: string, payload: UpdateInsurancePlanPayload) {
    return this.insuranceService.update(policyId, payload).pipe(map((response) => response.item as InsurancePlanItem));
  }
}
