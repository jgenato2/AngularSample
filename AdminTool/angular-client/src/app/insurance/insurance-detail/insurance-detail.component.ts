import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { finalize, forkJoin } from "rxjs";
import { AuthService } from "../../core/auth.service";
import { InsuranceFacade } from "../../features/insurance/application/insurance.facade";
import { InsuranceAuditLogItem, InsuranceFinancialAnalyticsItem, InsurancePlanItem, InsuranceStatusWorkflowItem } from "../../features/insurance/domain/insurance.models";
import { ClaimsFacade } from "../../features/claims/application/claims.facade";
import { ClaimItem } from "../../features/claims/domain/claim.models";
import { CoverageCardComponent } from "./components/coverage-card/coverage-card.component";
import { FinancialCardComponent } from "./components/financial-card/financial-card.component";
import { TimelineCardComponent } from "./components/timeline-card/timeline-card.component";
import { RiskModelCardComponent } from "./components/risk-model-card/risk-model-card.component";
import { BaselineProjectionCardComponent } from "./components/baseline-projection-card/baseline-projection-card.component";
import { StressTestCardComponent } from "./components/stress-test-card/stress-test-card.component";
import { AlgorithmNotesCardComponent } from "./components/algorithm-notes-card/algorithm-notes-card.component";
import { AuditLogSectionComponent } from "./components/audit-log-section/audit-log-section.component";

const DEFAULT_STATUS_WORKFLOW: Record<string, string[]> = {
  "New": ["Underwriting", "Cancelled"],
  "Underwriting": ["Pending Activation", "Cancelled"],
  "Pending Activation": ["Active", "Cancelled"],
  "Active": ["Grace Period", "Pending Renewal", "Suspended", "Cancelled", "Expired"],
  "Grace Period": ["Active", "Suspended", "Cancelled", "Expired"],
  "Pending Renewal": ["Renewed", "Expired", "Cancelled"],
  "Renewed": ["Active", "Cancelled"],
  "Suspended": ["Active", "Cancelled", "Expired"],
  "Cancelled": ["New"],
  "Expired": ["New"],
};

@Component({
  selector: "app-insurance-detail",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    CoverageCardComponent,
    FinancialCardComponent,
    TimelineCardComponent,
    RiskModelCardComponent,
    BaselineProjectionCardComponent,
    StressTestCardComponent,
    AlgorithmNotesCardComponent,
    AuditLogSectionComponent,
  ],
  templateUrl: "./insurance-detail.component.html",
  styleUrl: "./insurance-detail.component.scss",
})
export class InsuranceDetailComponent implements OnInit, OnDestroy {
  statusWorkflow: Record<string, string[]> = { ...DEFAULT_STATUS_WORKFLOW };
  plan: InsurancePlanItem | null = null;
  memberClaimId: string | null = null;
  financial: InsuranceFinancialAnalyticsItem | null = null;
  auditLogs: InsuranceAuditLogItem[] = [];
  auditLoading = false;
  activeTab: "details" | "financial" | "audit" = "details";
  showStatusModal = false;
  formModel = {
    memberName: "",
    provider: "",
    planType: "",
    status: "",
    monthlyPremium: 0,
    deductible: 0,
    outOfPocketMax: 0,
    effectiveDate: "",
    renewalDate: "",
    comments: "",
  };
  loading = false;
  saving = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly insuranceFacade: InsuranceFacade,
    private readonly claimsFacade: ClaimsFacade,
    private readonly router: Router,
    public readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadStatusWorkflow();
    this.load();
  }

  ngOnDestroy() {
    this.clearAlertTimer();
  }

  get canManage() {
    return this.auth.isAdmin();
  }

  switchTab(tab: "details" | "financial" | "audit") {
    this.activeTab = tab;
  }

  openStatusModal() {
    if (!this.canManage || this.saving) {
      return;
    }

    this.showStatusModal = true;
  }

  closeStatusModal() {
    if (this.saving) {
      return;
    }

    this.showStatusModal = false;
  }

  selectStatus(status: string) {
    if (this.saving) {
      return;
    }

    this.formModel.status = status;
    this.closeStatusModal();
  }

  get availableStatusOptions() {
    const current = this.plan?.status?.trim() || this.formModel.status?.trim();
    if (!current) {
      return Object.keys(this.statusWorkflow);
    }

    const transitions = this.statusWorkflow[current] ?? [];
    return transitions.includes(current) ? transitions : [current, ...transitions];
  }

  statusTagClass(status: string | null | undefined) {
    const value = (status ?? "").trim().toLowerCase();
    switch (value) {
      case "new":
        return "bg-secondary-subtle border border-secondary text-secondary";
      case "underwriting":
        return "bg-info text-dark";
      case "pending activation":
        return "bg-primary text-white";
      case "active":
        return "bg-success text-white";
      case "grace period":
        return "bg-warning text-dark";
      case "pending renewal":
        return "bg-warning-subtle border border-warning text-warning";
      case "renewed":
        return "bg-success-subtle border border-success text-success";
      case "suspended":
        return "bg-dark text-white";
      case "cancelled":
        return "bg-danger text-white";
      case "expired":
        return "bg-danger-subtle border border-danger text-danger";
      default:
        return "bg-light text-body";
    }
  }

  load() {
    const policyId = this.route.snapshot.paramMap.get("policyId");
    if (!policyId) {
      this.router.navigateByUrl("/insurance");
      return;
    }

    this.loading = true;
    forkJoin({
      plan: this.insuranceFacade.getByPolicyId(policyId),
      claims: this.claimsFacade.list(),
      financial: this.insuranceFacade.getFinancialAnalytics(policyId),
      auditLogs: this.insuranceFacade.getAuditLogs(policyId),
    })
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (result) => {
          this.plan = result.plan;
          this.memberClaimId = this.resolveClaimId(result.plan.policyId, result.claims);
          this.financial = result.financial;
          this.auditLogs = result.auditLogs;
          this.patchForm(result.plan);
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("Insurance plan not found.");
          this.router.navigateByUrl("/insurance");
        },
      });
  }

  save() {
    if (!this.plan || !this.canManage) {
      return;
    }

    this.saving = true;
    this.insuranceFacade
      .update(this.plan.policyId, {
        memberName: this.formModel.memberName,
        provider: this.formModel.provider,
        planType: this.formModel.planType,
        status: this.formModel.status,
        monthlyPremium: this.formModel.monthlyPremium,
        deductible: this.formModel.deductible,
        outOfPocketMax: this.formModel.outOfPocketMax,
        effectiveDate: this.formModel.effectiveDate,
        renewalDate: this.formModel.renewalDate,
        comments: this.formModel.comments,
      })
      .pipe(finalize(() => {
        this.saving = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (plan) => {
          this.plan = plan;
          this.patchForm(plan);
          this.reloadFinancialAnalytics(plan.policyId);
          this.reloadAuditLogs(plan.policyId);
          this.setAlert("Insurance plan updated.", "success");
        },
        error: () => this.setAlert("Failed to update insurance plan."),
      });
  }

  remove() {
    if (!this.plan || !this.canManage) {
      return;
    }

    if (!confirm(`Delete policy ${this.plan.policyId}?`)) {
      return;
    }

    this.insuranceFacade.remove(this.plan.policyId).subscribe({
      next: () => this.router.navigateByUrl("/insurance"),
      error: () => this.setAlert("Failed to delete insurance plan."),
    });
  }

  private patchForm(item: InsurancePlanItem) {
    this.formModel = {
      memberName: item.memberName,
      provider: item.provider,
      planType: item.planType,
      status: item.status,
      monthlyPremium: item.monthlyPremium,
      deductible: item.deductible,
      outOfPocketMax: item.outOfPocketMax,
      effectiveDate: item.effectiveDate?.slice(0, 10) ?? "",
      renewalDate: item.renewalDate?.slice(0, 10) ?? "",
      comments: item.comments ?? "",
    };
  }

  private resolveClaimId(policyId: string, claims: ClaimItem[]) {
    const match = claims.find((claim) => claim.policyId?.toLowerCase() === policyId.toLowerCase());
    return match?.claimId ?? null;
  }

  private reloadFinancialAnalytics(policyId: string) {
    this.insuranceFacade.getFinancialAnalytics(policyId).subscribe({
      next: (financial) => {
        this.financial = financial;
        this.cdr.markForCheck();
      },
    });
  }

  private reloadAuditLogs(policyId: string) {
    this.auditLoading = true;
    this.insuranceFacade.getAuditLogs(policyId)
      .pipe(finalize(() => {
        this.auditLoading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (auditLogs) => {
          this.auditLogs = auditLogs;
          this.cdr.markForCheck();
        },
      });
  }

  private loadStatusWorkflow() {
    this.insuranceFacade.getStatusWorkflow().subscribe({
      next: (response) => {
        this.statusWorkflow = this.toStatusWorkflowMap(response.workflow);
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.auth.logout();
          this.router.navigateByUrl("/login");
          return;
        }
        this.statusWorkflow = { ...DEFAULT_STATUS_WORKFLOW };
        this.cdr.markForCheck();
      },
    });
  }

  private toStatusWorkflowMap(items: InsuranceStatusWorkflowItem[] | null | undefined) {
    const result: Record<string, string[]> = {};
    for (const item of items ?? []) {
      if (!item?.status) {
        continue;
      }

      result[item.status] = [...(item.next ?? [])];
    }

    return Object.keys(result).length ? result : { ...DEFAULT_STATUS_WORKFLOW };
  }

  private setAlert(message: string, type: "danger" | "success" = "danger") {
    this.clearAlertTimer();
    this.alertMessage = message;
    this.alertType = type;
    this.alertTimerId = setTimeout(() => {
      this.alertMessage = null;
      this.cdr.markForCheck();
    }, 4000);
  }

  private clearAlertTimer() {
    if (!this.alertTimerId) {
      return;
    }

    clearTimeout(this.alertTimerId);
    this.alertTimerId = null;
  }
}
