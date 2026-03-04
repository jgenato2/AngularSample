import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";
import { CreateInsurancePlanPayload, InsurancePlanItem } from "../../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class CreateInsurancePlanCommand {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute(payload: CreateInsurancePlanPayload) {
    return this.insuranceService.create(payload).pipe(map((response) => response.item as InsurancePlanItem));
  }
}
