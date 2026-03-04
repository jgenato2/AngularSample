import { Inject, Injectable } from "@angular/core";
import { CreateInsurancePlanPayload } from "../../domain/insurance.models";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class CreateInsurancePlanCommand {
  constructor(@Inject(INSURANCE_REPOSITORY) private readonly insuranceRepository: InsuranceRepository) {}

  execute(payload: CreateInsurancePlanPayload) {
    return this.insuranceRepository.create(payload);
  }
}
