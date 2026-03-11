import { Injectable, inject } from "@angular/core";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class DeleteInsurancePlanCommand {
  private readonly insuranceRepository = inject<InsuranceRepository>(INSURANCE_REPOSITORY);


  constructor() {}

  execute(policyId: string) {
    return this.insuranceRepository.remove(policyId);
  }
}
