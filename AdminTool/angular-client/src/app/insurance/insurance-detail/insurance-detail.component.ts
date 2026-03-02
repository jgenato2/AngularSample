import { ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { finalize } from "rxjs";
import { AuthService } from "../../core/auth.service";
import { InsurancePlanItem, InsuranceService } from "../insurance.service";

@Component({
  selector: "app-insurance-detail",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  providers: [CurrencyPipe, DatePipe],
  templateUrl: "./insurance-detail.component.html",
  styleUrl: "./insurance-detail.component.scss",
})
export class InsuranceDetailComponent implements OnInit {
  plan: InsurancePlanItem | null = null;
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

  constructor(
    private readonly route: ActivatedRoute,
    private readonly insuranceService: InsuranceService,
    private readonly router: Router,
    public readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef,
    public readonly datePipe: DatePipe,
    public readonly currencyPipe: CurrencyPipe,
  ) {}

  ngOnInit() {
    this.load();
  }

  get canManage() {
    return this.auth.isAdmin();
  }

  load() {
    const policyId = this.route.snapshot.paramMap.get("policyId");
    if (!policyId) {
      this.router.navigateByUrl("/insurance");
      return;
    }

    this.loading = true;
    this.insuranceService
      .getByPolicyId(policyId)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.plan = response.item;
          this.patchForm(response.item);
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

  private setAlert(message: string, type: "danger" | "success" = "danger") {
    this.alertMessage = message;
    this.alertType = type;
  }
}
