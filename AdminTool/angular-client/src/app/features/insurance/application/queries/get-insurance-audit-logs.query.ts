import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";
import { InsuranceAuditLogItem } from "../../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class GetInsuranceAuditLogsQuery {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute(policyId: string) {
    return this.insuranceService.getAuditLogs(policyId).pipe(map((response) => (response.items ?? []) as InsuranceAuditLogItem[]));
  }
}
