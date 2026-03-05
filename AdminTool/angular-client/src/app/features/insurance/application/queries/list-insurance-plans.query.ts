import { Inject, Injectable } from "@angular/core";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class ListInsurancePlansQuery {
  constructor(@Inject(INSURANCE_REPOSITORY) private readonly insuranceRepository: InsuranceRepository) {}

  execute(sort?: Array<{ field: string; direction: "asc" | "desc" }>) {
    return this.insuranceRepository.listPlans(sort);
  }
}
