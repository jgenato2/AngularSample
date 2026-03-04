import { Inject, Injectable } from "@angular/core";
import { UpdateInsurancePlanPayload } from "../../domain/insurance.models";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class UpdateInsurancePlanCommand {
  constructor(@Inject(INSURANCE_REPOSITORY) private readonly insuranceRepository: InsuranceRepository) {}

  execute(policyId: string, payload: UpdateInsurancePlanPayload) {
    return this.insuranceRepository.update(policyId, payload);
  }
}
