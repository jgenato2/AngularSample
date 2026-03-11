import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { CellClickedEvent, ColDef } from "ag-grid-community";
import { finalize, forkJoin, Subscription } from "rxjs";
import { AuthService } from "../../../core/auth.service";
import { ClaimsFacade } from "../../../features/claims/application/claims.facade";
import { InsuranceFacade } from "../../../features/insurance/application/insurance.facade";
import { InsurancePlanItem } from "../../../features/insurance/domain/insurance.models";
import { DataGridComponent, GridSortState } from "../../../shared/data-grid/data-grid.component";
import { SearchQueryComponent } from "../../../shared/search-query/search-query.component";
import { SummaryStatItem, SummaryStatsComponent } from "../../../shared/summary-stats/summary-stats.component";
import { ProvidersService } from "../../providers/providers.service";

const DEFAULT_CREATE_STATUS_OPTIONS = ["New"];

@Component({
  selector: "app-insurance-list",
  standalone: true,
  imports: [CommonModule, FormsModule, DataGridComponent, SearchQueryComponent, SummaryStatsComponent],
  providers: [CurrencyPipe, DatePipe],
  templateUrl: "./insurance-list.component.html",
  styleUrl: "./insurance-list.component.scss",
})
export class InsuranceListComponent implements OnInit, OnDestroy {
  private readonly insuranceFacade = inject(InsuranceFacade);
  private readonly claimsFacade = inject(ClaimsFacade);
  private readonly providersService = inject(ProvidersService);
  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly currencyPipe = inject(CurrencyPipe);
  readonly datePipe = inject(DatePipe);

  createStatusOptions = [...DEFAULT_CREATE_STATUS_OPTIONS];
  plans: InsurancePlanItem[] = [];
  providerOptions: string[] = [];
  gridSearchQuery = "";
  gridSort: GridSortState[] = [];
  loading = false;
  creating = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private claimIdByPolicyId = new Map<string, string>();
  private listRequestSub: Subscription | null = null;
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;
  createModel = {
    policyId: "",
    memberName: "",
    provider: "",
    planType: "",
    status: "New",
    monthlyPremium: 0,
    deductible: 0,
    outOfPocketMax: 0,
    effectiveDate: "",
    renewalDate: "",
  };


  constructor() {}

  ngOnInit() {
    this.gridSearchQuery = this.route.snapshot.queryParamMap.get("query") ?? "";
    this.loadProviderOptions();
    this.loadStatusWorkflow();
    this.load();
  }

  ngOnDestroy() {
    this.listRequestSub?.unsubscribe();
    this.clearAlertTimer();
  }

  columnDefs: ColDef<InsurancePlanItem>[] = [
    { field: "policyId", headerName: "Policy ID", width: 150, minWidth: 140, sort: "desc", sortIndex: 0 },
    { field: "memberName", headerName: "Member", width: 210, minWidth: 180 },
    { field: "provider", headerName: "Provider", width: 210, minWidth: 180 },
    { field: "planType", headerName: "Plan", width: 170, minWidth: 150 },
    {
      field: "status",
      headerName: "Status",
      width: 170,
      minWidth: 150,
      cellClass: (params) => this.statusCellClass(params.value),
    },
    {
      field: "monthlyPremium",
      headerName: "Premium",
      width: 150,
      minWidth: 140,
      valueFormatter: (params) => this.formatAccountingCurrency(params.value, "1.2-2"),
      cellClass: (params) => this.negativeValueClass(params.value),
    },
    {
      field: "deductible",
      headerName: "Deductible",
      width: 150,
      minWidth: 140,
      valueFormatter: (params) => this.formatAccountingCurrency(params.value, "1.0-0"),
      cellClass: (params) => this.negativeValueClass(params.value),
    },
    {
      field: "outOfPocketMax",
      headerName: "Out-of-pocket Max",
      width: 190,
      minWidth: 170,
      valueFormatter: (params) => this.formatAccountingCurrency(params.value, "1.0-0"),
      cellClass: (params) => this.negativeValueClass(params.value),
    },
    {
      field: "effectiveDate",
      headerName: "Effective",
      width: 150,
      minWidth: 130,
      sort: "desc",
      sortIndex: 1,
      valueFormatter: (params) => this.datePipe.transform(params.value, "MMM d, y") ?? "",
    },
    {
      field: "renewalDate",
      headerName: "Renewal",
      width: 150,
      minWidth: 140,
      valueFormatter: (params) => this.datePipe.transform(params.value, "MMM d, y") ?? "",
    },
  ];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };

  load() {
    this.loading = true;
    this.alertMessage = null;
    this.cdr.markForCheck();
    this.listRequestSub?.unsubscribe();

    this.listRequestSub = forkJoin({
      plans: this.insuranceFacade.listPlans(this.gridSort),
      claims: this.claimsFacade.list(),
    })
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (result) => {
          this.plans = [...result.plans];
          this.claimIdByPolicyId.clear();
          for (const claim of result.claims) {
            const policyId = String(claim.policyId ?? "").trim().toLowerCase();
            if (!policyId || this.claimIdByPolicyId.has(policyId)) {
              continue;
            }
            this.claimIdByPolicyId.set(policyId, claim.claimId);
          }
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("Failed to load health insurance sample data.");
          this.cdr.markForCheck();
        },
      });
  }
  private loadStatusWorkflow() {
    this.insuranceFacade.getStatusWorkflow().subscribe({
      next: (response) => {
        this.createStatusOptions = [...(response.createStatuses?.length ? response.createStatuses : DEFAULT_CREATE_STATUS_OPTIONS)];
        if (!this.createStatusOptions.includes(this.createModel.status)) {
          this.createModel.status = this.createStatusOptions[0] ?? "Draft";
        }
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.auth.logout();
          this.router.navigateByUrl("/login");
          return;
        }
        this.createStatusOptions = [...DEFAULT_CREATE_STATUS_OPTIONS];
        this.cdr.markForCheck();
      },
    });
  }

  private formatAccountingCurrency(value: unknown, digits: string) {
    const amount = typeof value === "number" ? value : Number(value ?? 0);
    const formatted = this.currencyPipe.transform(Math.abs(amount), "USD", "symbol", digits) ?? "$0";
    return amount < 0 ? `(${formatted})` : formatted;
  }

  private negativeValueClass(value: unknown) {
    const amount = typeof value === "number" ? value : Number(value ?? 0);
    return amount < 0 ? "text-danger" : "";
  }

  private statusCellClass(value: unknown) {
    const status = String(value ?? "").toLowerCase();
    if (status.includes("active")) {
      return "text-success";
    }

    if (status.includes("pending")) {
      return "text-warning";
    }

    if (status.includes("expired")) {
      return "text-danger";
    }

    return "text-body-secondary";
  }

  openDetail(plan?: InsurancePlanItem | null) {
    if (!plan) {
      return;
    }
    this.router.navigate(["/insurance", plan.policyId]);
  }

  get searchMatches(): InsurancePlanItem[] {
    const query = this.gridSearchQuery.trim().toLowerCase();
    if (!query) {
      return [];
    }

    return this.plans.filter((plan) =>
      [plan.policyId, plan.memberName, plan.provider, plan.planType, plan.status].some((value) => String(value ?? "").toLowerCase().includes(query)),
    );
  }

  readonly insuranceMatchIdentity = (match: unknown) => String((match as InsurancePlanItem | null)?.policyId ?? "");

  onSearchMatchOpened(match: unknown) {
    this.openDetail(match as InsurancePlanItem);
  }

  onGridSortChanged(sortState: GridSortState[]) {
    this.gridSort = [...sortState];
    this.load();
  }

  onCellClicked(event: CellClickedEvent<InsurancePlanItem>) {
    const field = event.colDef.field;
    const row = event.data;
    if (!field || !row) {
      return;
    }

    if (field === "provider" && row.provider) {
      event.event?.stopPropagation();
      this.router.navigate(["/providers", row.provider]);
      return;
    }

    if (field === "memberName") {
      const claimId = this.claimIdByPolicyId.get(String(row.policyId ?? "").trim().toLowerCase());
      if (!claimId) {
        this.setAlert("No linked claim record found for this member.");
        return;
      }
      event.event?.stopPropagation();
      this.router.navigate(["/claims", claimId]);
    }
  }

  get summaryItems(): SummaryStatItem[] {
    let activePlans = 0;
    let pendingPlans = 0;
    let expiredPlans = 0;

    for (const plan of this.plans) {
      const status = String(plan.status ?? "").toLowerCase();
      if (status.includes("active")) {
        activePlans += 1;
      } else if (status.includes("pending")) {
        pendingPlans += 1;
      } else if (status.includes("expired")) {
        expiredPlans += 1;
      }
    }

    return [
      { label: "Total plans", value: this.plans.length },
      { label: "Active plans", value: activePlans, tone: "success" },
      { label: "Pending plans", value: pendingPlans, tone: "warning" },
      { label: "Expired plans", value: expiredPlans, tone: "danger" },
    ];
  }

  createPlan() {
    if (!this.auth.isAdmin()) {
      return;
    }

    this.creating = true;
    this.insuranceFacade
      .create({
        policyId: this.createModel.policyId,
        memberName: this.createModel.memberName,
        provider: this.createModel.provider,
        planType: this.createModel.planType,
        status: this.createModel.status,
        monthlyPremium: this.createModel.monthlyPremium,
        deductible: this.createModel.deductible,
        outOfPocketMax: this.createModel.outOfPocketMax,
        effectiveDate: this.createModel.effectiveDate,
        renewalDate: this.createModel.renewalDate,
      })
      .pipe(finalize(() => {
        this.creating = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: () => {
          this.setAlert("Insurance plan created.", "success");
          this.createModel = {
            policyId: "",
            memberName: "",
            provider: "",
            planType: "",
            status: "New",
            monthlyPremium: 0,
            deductible: 0,
            outOfPocketMax: 0,
            effectiveDate: "",
            renewalDate: "",
          };
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert(error.error?.message ?? "Failed to create insurance plan.");
          this.cdr.markForCheck();
        },
      });
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

  private loadProviderOptions() {
    this.providersService.list().subscribe({
      next: (response) => {
        this.providerOptions = [...new Set((response.items ?? []).map((item) => item.provider).filter((provider) => !!provider))]
          .sort((a, b) => a.localeCompare(b));
        this.cdr.markForCheck();
      },
      error: () => {
        this.providerOptions = [];
        this.cdr.markForCheck();
      },
    });
  }
}

