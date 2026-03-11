import { Injectable, inject } from "@angular/core";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class ListInsurancePlansQuery {
  private readonly insuranceRepository = inject<InsuranceRepository>(INSURANCE_REPOSITORY);


  constructor() {}

  execute(sort?: Array<{ field: string; direction: "asc" | "desc" }>) {
    return this.insuranceRepository.listPlans(sort);
  }
}
