import { Injectable } from "@angular/core";
import { InsuranceService } from "../../../../insurance/insurance.service";

@Injectable({ providedIn: "root" })
export class GetInsuranceStatusWorkflowQuery {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute() {
    return this.insuranceService.getStatusWorkflow() as ReturnType<InsuranceService["getStatusWorkflow"]>;
  }
}
