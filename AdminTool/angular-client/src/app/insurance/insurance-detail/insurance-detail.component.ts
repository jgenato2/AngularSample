import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { finalize, forkJoin } from "rxjs";
import { AuthService } from "../../core/auth.service";
import { InsuranceFinancialAnalyticsItem, InsurancePlanItem, InsuranceService, InsuranceStatusWorkflowItem } from "../insurance.service";

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
  imports: [CommonModule, FormsModule, RouterLink],
  providers: [CurrencyPipe, DatePipe, DecimalPipe],
  templateUrl: "./insurance-detail.component.html",
  styleUrl: "./insurance-detail.component.scss",
})
export class InsuranceDetailComponent implements OnInit, OnDestroy {
  statusWorkflow: Record<string, string[]> = { ...DEFAULT_STATUS_WORKFLOW };
  plan: InsurancePlanItem | null = null;
  financial: InsuranceFinancialAnalyticsItem | null = null;
  activeTab: "details" | "financial" = "details";
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
  };
  loading = false;
  saving = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly insuranceService: InsuranceService,
    private readonly router: Router,
    public readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef,
    public readonly datePipe: DatePipe,
    public readonly currencyPipe: CurrencyPipe,
    public readonly decimalPipe: DecimalPipe,
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

  switchTab(tab: "details" | "financial") {
    this.activeTab = tab;
  }

  openStatusModal() {
    if (!this.canManage || this.saving) {
      return;
    }

    this.showStatusModal = true;
  }

  closeStatusModal() {
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

  formatCurrency(value: number | null | undefined, digits: string = "1.2-2") {
    const amount = value ?? 0;
    const formatted = this.currencyPipe.transform(Math.abs(amount), "USD", "symbol", digits) ?? "$0.00";
    return amount < 0 ? `(${formatted})` : formatted;
  }

  formatPercent(value: number | null | undefined, digits: string = "1.1-1") {
    const amount = value ?? 0;
    const formatted = this.decimalPipe.transform(Math.abs(amount), digits) ?? "0.0";
    return amount < 0 ? `(${formatted}%)` : `${formatted}%`;
  }

  signedValueClass(value: number | null | undefined) {
    return (value ?? 0) < 0 ? "text-danger" : "";
  }

  policyStatusClass(status: string | null | undefined) {
    const value = (status ?? "").trim().toLowerCase();
    switch (value) {
      case "new":
        return "text-secondary";
      case "underwriting":
        return "text-info";
      case "pending activation":
        return "text-primary";
      case "active":
      case "renewed":
        return "text-success";
      case "grace period":
      case "pending renewal":
        return "text-warning";
      case "suspended":
        return "text-dark";
      case "cancelled":
      case "expired":
        return "text-danger";
      default:
        return "text-body-secondary";
    }
  }

  riskBandClass(riskBand: string | null | undefined) {
    const value = (riskBand ?? "").toLowerCase();
    if (value === "low") {
      return "text-success";
    }

    if (value === "moderate") {
      return "text-warning";
    }

    if (value === "high") {
      return "text-danger";
    }

    return "text-body-secondary";
  }

  load() {
    const policyId = this.route.snapshot.paramMap.get("policyId");
    if (!policyId) {
      this.router.navigateByUrl("/insurance");
      return;
    }

    this.loading = true;
    forkJoin({
      plan: this.insuranceService.getByPolicyId(policyId),
      financial: this.insuranceService.getFinancialAnalytics(policyId),
    })
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (result) => {
          this.plan = result.plan.item;
          this.financial = result.financial.item;
          this.patchForm(result.plan.item);
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
    this.insuranceService
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
      })
      .pipe(finalize(() => {
        this.saving = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.plan = response.item;
          this.patchForm(response.item);
          this.reloadFinancialAnalytics(response.item.policyId);
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

    this.insuranceService.remove(this.plan.policyId).subscribe({
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
    };
  }

  private reloadFinancialAnalytics(policyId: string) {
    this.insuranceService.getFinancialAnalytics(policyId).subscribe({
      next: (response) => {
        this.financial = response.item;
        this.cdr.markForCheck();
      },
    });
  }

  private loadStatusWorkflow() {
    this.insuranceService.getStatusWorkflow().subscribe({
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
