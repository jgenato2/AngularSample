import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { finalize, forkJoin } from "rxjs";
import { AuthService } from "../../../core/auth.service";
import { ClaimsFacade } from "../../../features/claims/application/claims.facade";
import { ClaimAuditLogItem, ClaimItem, ClaimStatusWorkflowItem } from "../../../features/claims/domain/claim.models";
import { InsuranceFacade } from "../../../features/insurance/application/insurance.facade";
import { AuditLogSectionComponent } from "../../insurance/insurance-detail/components/audit-log-section/audit-log-section.component";

const DEFAULT_STATUS_WORKFLOW: Record<string, string[]> = {
  Submitted: ["Under Review", "Rejected"],
  "Under Review": ["Approved", "Rejected"],
  Approved: ["Approved"],
  Rejected: ["Submitted"],
};

@Component({
  selector: "app-claim-detail",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AuditLogSectionComponent],
  providers: [CurrencyPipe, DatePipe],
  templateUrl: "./claim-detail.component.html",
  styleUrl: "./claim-detail.component.scss",
})
export class ClaimDetailComponent implements OnInit, OnDestroy {
  statusWorkflow: Record<string, string[]> = { ...DEFAULT_STATUS_WORKFLOW };
  policyIdOptions: string[] = [];
  claim: ClaimItem | null = null;
  auditLogs: ClaimAuditLogItem[] = [];
  auditLoading = false;
  activeTab: "details" | "audit" = "details";
  showStatusModal = false;
  loading = false;
  saving = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  formModel = {
    policyId: "",
    memberName: "",
    provider: "",
    claimType: "Outpatient",
    serviceCategory: "",
    diagnosisCode: "",
    submittedAt: "",
    serviceDate: "",
    claimAmount: 0,
    status: "Submitted",
    notes: "",
  };

  constructor(
    private readonly route: ActivatedRoute,
    private readonly claimsFacade: ClaimsFacade,
    private readonly insuranceFacade: InsuranceFacade,
    private readonly router: Router,
    public readonly auth: AuthService,
    public readonly currencyPipe: CurrencyPipe,
    public readonly datePipe: DatePipe,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadPolicyIdOptions();
    this.loadStatusWorkflow();
    this.load();
  }

  ngOnDestroy() {
    this.clearAlertTimer();
  }

  get canManage() {
    return this.auth.isAdmin();
  }

  switchTab(tab: "details" | "audit") {
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
    const current = this.claim?.status?.trim() || this.formModel.status?.trim();
    if (!current) {
      return [];
    }

    const transitions = this.statusWorkflow[current] ?? [];
    return transitions.includes(current) ? transitions : [current, ...transitions];
  }

  statusTagClass(status: string | null | undefined) {
    const value = (status ?? "").trim().toLowerCase();
    switch (value) {
      case "submitted":
        return "bg-secondary-subtle border border-secondary text-secondary";
      case "under review":
        return "bg-warning text-dark";
      case "approved":
        return "bg-success text-white";
      case "rejected":
        return "bg-danger text-white";
      default:
        return "bg-light text-body";
    }
  }

  load() {
    const claimId = this.route.snapshot.paramMap.get("claimId");
    if (!claimId) {
      return;
    }

    this.loading = true;
    this.auditLoading = true;
    forkJoin({
      claim: this.claimsFacade.getById(claimId),
      auditLogs: this.claimsFacade.getAuditLogs(claimId),
    })
      .pipe(finalize(() => {
        this.loading = false;
        this.auditLoading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (result) => {
          this.claim = result.claim;
          this.auditLogs = result.auditLogs;
          this.patchForm(result.claim);
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("Claim not found.");
          this.router.navigateByUrl("/claims");
        },
      });
  }

  save() {
    if (!this.claim || !this.canManage) {
      return;
    }

    this.saving = true;
    this.claimsFacade
      .update(this.claim.claimId, {
        policyId: this.formModel.policyId,
        memberName: this.formModel.memberName,
        provider: this.formModel.provider,
        claimType: this.formModel.claimType,
        serviceCategory: this.formModel.serviceCategory,
        diagnosisCode: this.formModel.diagnosisCode,
        submittedAt: this.formModel.submittedAt,
        serviceDate: this.formModel.serviceDate,
        claimAmount: this.formModel.claimAmount,
        status: this.formModel.status,
        notes: this.formModel.notes,
      })
      .pipe(finalize(() => {
        this.saving = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (claim) => {
          this.claim = claim;
          this.patchForm(claim);
          this.reloadAuditLogs(claim.claimId);
          this.setAlert("Claim updated.", "success");
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert(error.error?.message ?? "Failed to update claim.");
        },
      });
  }

  remove() {
    if (!this.claim || !this.canManage) {
      return;
    }

    if (!confirm(`Delete claim ${this.claim.claimId}?`)) {
      return;
    }

    this.claimsFacade.remove(this.claim.claimId).subscribe({
      next: () => this.router.navigateByUrl("/claims"),
      error: () => this.setAlert("Failed to delete claim."),
    });
  }

  private patchForm(claim: ClaimItem) {
    this.formModel = {
      policyId: claim.policyId,
      memberName: claim.memberName,
      provider: claim.provider,
      claimType: claim.claimType,
      serviceCategory: claim.serviceCategory,
      diagnosisCode: claim.diagnosisCode,
      submittedAt: claim.submittedAt?.slice(0, 10) ?? "",
      serviceDate: claim.serviceDate?.slice(0, 10) ?? "",
      claimAmount: claim.claimAmount,
      status: claim.status,
      notes: claim.notes ?? "",
    };
  }

  private loadStatusWorkflow() {
    this.claimsFacade.getStatusWorkflow().subscribe({
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

  private loadPolicyIdOptions() {
    this.insuranceFacade.listPlans().subscribe({
      next: (plans) => {
        this.policyIdOptions = [...new Set(plans.map((plan) => plan.policyId).filter((policyId) => !!policyId))].sort((a, b) => a.localeCompare(b));
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.auth.logout();
          this.router.navigateByUrl("/login");
          return;
        }
        this.policyIdOptions = [];
        this.cdr.markForCheck();
      },
    });
  }

  private reloadAuditLogs(claimId: string) {
    this.auditLoading = true;
    this.claimsFacade.getAuditLogs(claimId)
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

  private toStatusWorkflowMap(items: ClaimStatusWorkflowItem[] | null | undefined) {
    const map: Record<string, string[]> = {};
    for (const item of items ?? []) {
      const status = item.status?.trim();
      if (!status) {
        continue;
      }

      map[status] = [...(item.next ?? [])];
    }

    return map;
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

