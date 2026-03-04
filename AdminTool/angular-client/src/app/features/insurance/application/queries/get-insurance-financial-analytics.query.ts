import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";
import { InsuranceFinancialAnalyticsItem } from "../../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class GetInsuranceFinancialAnalyticsQuery {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute(policyId: string) {
    return this.insuranceService
      .getFinancialAnalytics(policyId)
      .pipe(map((response) => response.item as InsuranceFinancialAnalyticsItem));
  }
}
