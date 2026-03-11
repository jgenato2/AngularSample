import { Injectable, inject } from "@angular/core";
import { CreateInsurancePlanPayload } from "../../domain/insurance.models";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class CreateInsurancePlanCommand {
  private readonly insuranceRepository = inject<InsuranceRepository>(INSURANCE_REPOSITORY);


  constructor() {}

  execute(payload: CreateInsurancePlanPayload) {
    return this.insuranceRepository.create(payload);
  }
}
