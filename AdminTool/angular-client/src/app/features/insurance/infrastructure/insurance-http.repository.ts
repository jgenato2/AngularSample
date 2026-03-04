import { Injectable } from "@angular/core";
import { Observable, map } from "rxjs";
import { InsuranceService } from "../../../pages/insurance/insurance.service";
import {
  CreateInsurancePlanPayload,
  InsuranceAuditLogItem,
  InsuranceFinancialAnalyticsItem,
  InsurancePlanItem,
  InsuranceStatusWorkflowResponse,
  UpdateInsurancePlanPayload,
} from "../domain/insurance.models";
import { InsuranceRepository } from "../domain/insurance.repository";

@Injectable({ providedIn: "root" })
export class InsuranceHttpRepository implements InsuranceRepository {
  constructor(private readonly insuranceService: InsuranceService) {}

  listPlans() {
    return this.insuranceService.listPlans().pipe(map((response) => response.items ?? []));
  }

  getByPolicyId(policyId: string) {
    return this.insuranceService.getByPolicyId(policyId).pipe(map((response) => response.item as InsurancePlanItem));
  }

  getFinancialAnalytics(policyId: string) {
    return this.insuranceService
      .getFinancialAnalytics(policyId)
      .pipe(map((response) => response.item as InsuranceFinancialAnalyticsItem));
  }

  getAuditLogs(policyId: string) {
    return this.insuranceService.getAuditLogs(policyId).pipe(map((response) => (response.items ?? []) as InsuranceAuditLogItem[]));
  }

  getListAccessAuditLogs() {
    return this.insuranceService.getListAccessAuditLogs().pipe(map((response) => (response.items ?? []) as InsuranceAuditLogItem[]));
  }

  getStatusWorkflow() {
    return this.insuranceService.getStatusWorkflow() as Observable<InsuranceStatusWorkflowResponse>;
  }

  create(payload: CreateInsurancePlanPayload) {
    return this.insuranceService.create(payload).pipe(map((response) => response.item as InsurancePlanItem));
  }

  update(policyId: string, payload: UpdateInsurancePlanPayload) {
    return this.insuranceService.update(policyId, payload).pipe(map((response) => response.item as InsurancePlanItem));
  }

  remove(policyId: string) {
    return this.insuranceService.remove(policyId).pipe(map((response) => !!response.ok));
  }
}
