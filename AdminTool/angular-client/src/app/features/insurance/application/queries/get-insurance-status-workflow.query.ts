import { Inject, Injectable } from "@angular/core";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class GetInsuranceStatusWorkflowQuery {
  constructor(@Inject(INSURANCE_REPOSITORY) private readonly insuranceRepository: InsuranceRepository) {}

  execute() {
    return this.insuranceRepository.getStatusWorkflow();
  }
}
