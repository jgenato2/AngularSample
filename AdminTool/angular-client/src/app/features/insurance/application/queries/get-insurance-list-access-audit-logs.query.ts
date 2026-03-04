import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { InsuranceService } from "../../../../insurance/insurance.service";
import { InsuranceAuditLogItem } from "../../domain/insurance.models";

@Injectable({ providedIn: "root" })
export class GetInsuranceListAccessAuditLogsQuery {
  constructor(private readonly insuranceService: InsuranceService) {}

  execute() {
    return this.insuranceService.getListAccessAuditLogs().pipe(map((response) => (response.items ?? []) as InsuranceAuditLogItem[]));
  }
}
