import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";

@Injectable({ providedIn: "root" })
export class ListInsurancePlansQuery {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute() {
    return this.insuranceService.listPlans().pipe(map((response) => response.items ?? []));
  }
}
