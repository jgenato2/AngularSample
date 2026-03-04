import { Inject, Injectable } from "@angular/core";
import { INSURANCE_REPOSITORY, InsuranceRepository } from "../../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class GetInsuranceFinancialAnalyticsQuery {
  constructor(@Inject(INSURANCE_REPOSITORY) private readonly insuranceRepository: InsuranceRepository) {}

  execute(policyId: string) {
    return this.insuranceRepository.getFinancialAnalytics(policyId);
  }
}
