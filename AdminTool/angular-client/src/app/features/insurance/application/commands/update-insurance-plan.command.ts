import { Injectable, inject } from "@angular/core";
import { UpdateInsurancePlanPayload } from "../../domain/insurance.models";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class UpdateInsurancePlanCommand {
  private readonly insuranceRepository = inject<InsuranceRepository>(INSURANCE_REPOSITORY);


  constructor() {}

  execute(policyId: string, payload: UpdateInsurancePlanPayload) {
    return this.insuranceRepository.update(policyId, payload);
  }
}
