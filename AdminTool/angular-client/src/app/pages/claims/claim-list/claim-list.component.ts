import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { Router } from "@angular/router";
import { CellClickedEvent, ColDef } from "ag-grid-community";
import { finalize, Subscription } from "rxjs";
import { AuthService } from "../../../core/auth.service";
import { ClaimsFacade } from "../../../features/claims/application/claims.facade";
import { ClaimItem, ClaimStatusWorkflowItem, CreateClaimPayload } from "../../../features/claims/domain/claim.models";
import { InsuranceFacade } from "../../../features/insurance/application/insurance.facade";
import { InsurancePlanItem } from "../../../features/insurance/domain/insurance.models";
import { DataGridComponent, GridSortState } from "../../../shared/data-grid/data-grid.component";
import { SearchQueryComponent } from "../../../shared/search-query/search-query.component";

const DEFAULT_CREATE_STATUS_OPTIONS = ["Submitted"];

@Component({
  selector: "app-claim-list",
  standalone: true,
  imports: [CommonModule, FormsModule, DataGridComponent, SearchQueryComponent],
  providers: [CurrencyPipe, DatePipe],
  templateUrl: "./claim-list.component.html",
  styleUrl: "./claim-list.component.scss",
})
export class ClaimListComponent implements OnInit, OnDestroy {
  private readonly claimsFacade = inject(ClaimsFacade);
  private readonly insuranceFacade = inject(InsuranceFacade);
  readonly auth = inject(AuthService);
  readonly currencyPipe = inject(CurrencyPipe);
  readonly datePipe = inject(DatePipe);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  createStatusOptions = [...DEFAULT_CREATE_STATUS_OPTIONS];
  statusWorkflow: Record<string, string[]> = {};
  insurancePlans: InsurancePlanItem[] = [];
  policyIdOptions: string[] = [];
  claims: ClaimItem[] = [];
  displayedClaims: ClaimItem[] = [];
  gridSearchQuery = "";
  gridSort: GridSortState[] = [];
  loading = false;
  creating = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private listRequestSub: Subscription | null = null;

  createModel: CreateClaimPayload = {
    claimId: "",
    policyId: "",
    memberName: "",
    provider: "",
    claimType: "Outpatient",
    serviceCategory: "Diagnostics",
    diagnosisCode: "",
    submittedAt: "",
    serviceDate: "",
    claimAmount: 0,
    status: "Submitted",
    notes: "",
  };


  constructor() {}

  ngOnInit() {
    this.loadPolicyIdOptions();
    this.loadStatusWorkflow();
    this.load();
  }

  ngOnDestroy() {
    this.listRequestSub?.unsubscribe();
  }

  

  columnDefs: ColDef<ClaimItem>[] = [
    { field: "claimId", headerName: "Claim ID", width: 150, minWidth: 140, sort: "desc", sortIndex: 0 },
    { field: "policyId", headerName: "Policy ID", width: 150, minWidth: 140 },
    { field: "memberName", headerName: "Member", width: 210, minWidth: 180 },
    { field: "provider", headerName: "Provider", width: 210, minWidth: 180 },
    { field: "claimType", headerName: "Claim Type", width: 170, minWidth: 150 },
    { field: "serviceCategory", headerName: "Service Category", width: 190, minWidth: 170 },
    { field: "diagnosisCode", headerName: "Dx Code", width: 140, minWidth: 120 },
    {
      field: "submittedAt",
      headerName: "Submitted",
      width: 150,
      minWidth: 130,
      sort: "desc",
      sortIndex: 1,
      valueFormatter: (params) => this.datePipe.transform(params.value, "MMM d, y") ?? "",
    },
    {
      field: "serviceDate",
      headerName: "Service Date",
      width: 150,
      minWidth: 130,
      valueFormatter: (params) => this.datePipe.transform(params.value, "MMM d, y") ?? "",
    },
    {
      field: "claimAmount",
      headerName: "Amount",
      width: 150,
      minWidth: 140,
      valueFormatter: (params) => this.formatAccountingCurrency(params.value, "1.2-2"),
      cellClass: (params) => this.negativeValueClass(params.value),
    },
    {
      field: "status",
      headerName: "Status",
      width: 170,
      minWidth: 150,
      cellClass: (params) => this.statusCellClass(params.value),
    },
    { field: "notes", headerName: "Notes", width: 260, minWidth: 220 },
  ];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };

  load() {
    this.loading = true;
    this.alertMessage = null;
    this.listRequestSub?.unsubscribe();

    this.listRequestSub = this.claimsFacade
      .list(this.gridSort)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (items) => {
          this.claims = [...items];
          this.displayedClaims = [...items];
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("Failed to load claims.");
          this.cdr.markForCheck();
        },
      });
  }

  create() {
    if (!this.auth.isAdmin()) {
      return;
    }

    this.creating = true;
    this.alertMessage = null;

    this.claimsFacade
      .create(this.createModel)
      .pipe(finalize(() => {
        this.creating = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: () => {
          this.setAlert("Claim created.", "success");
          this.resetCreateModel();
          this.load();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert(error.error?.message ?? "Failed to create claim.");
        },
      });
  }

  private resetCreateModel() {
    this.createModel = {
      claimId: "",
      policyId: "",
      memberName: "",
      provider: "",
      claimType: "Outpatient",
      serviceCategory: "Diagnostics",
      diagnosisCode: "",
      submittedAt: "",
      serviceDate: "",
      claimAmount: 0,
      status: this.createStatusOptions[0] ?? "Submitted",
      notes: "",
    };
  }

  private loadStatusWorkflow() {
    this.claimsFacade.getStatusWorkflow().subscribe({
      next: (response) => {
        this.createStatusOptions = [...(response.createStatuses?.length ? response.createStatuses : DEFAULT_CREATE_STATUS_OPTIONS)];
        this.statusWorkflow = this.toStatusWorkflowMap(response.workflow);
        if (!this.createStatusOptions.includes(this.createModel.status)) {
          this.createModel.status = this.createStatusOptions[0] ?? "Submitted";
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
        this.statusWorkflow = {};
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
    this.alertMessage = message;
    this.alertType = type;
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
    if (status.includes("approved")) {
      return "text-success";
    }

    if (status.includes("review")) {
      return "text-warning";
    }

    if (status.includes("rejected")) {
      return "text-danger";
    }

    if (status.includes("submitted")) {
      return "text-secondary";
    }

    return "text-body-secondary";
  }

  openDetail(claim?: ClaimItem | null) {
    if (!claim) {
      return;
    }
    this.router.navigate(["/claims", claim.claimId]);
  }

  onCellClicked(event: CellClickedEvent<ClaimItem>) {
    if (event.colDef.field !== "policyId") {
      return;
    }

    const policyId = event.data?.policyId;
    if (!policyId) {
      return;
    }

    event.event?.stopPropagation();
    this.router.navigate(["/insurance", policyId]);
  }

  get insuranceSearchMatches(): InsurancePlanItem[] {
    const query = this.gridSearchQuery.trim().toLowerCase();
    if (!query) {
      return [];
    }

    return this.insurancePlans.filter((plan) =>
      [plan.policyId, plan.memberName, plan.provider, plan.planType, plan.status].some((value) => String(value ?? "").toLowerCase().includes(query)),
    );
  }

  readonly insurancePolicyMatchIdentity = (match: unknown) => String((match as InsurancePlanItem | null)?.policyId ?? "");

  onInsuranceSearchMatchOpened(match: unknown) {
    const policyId = String((match as InsurancePlanItem | null)?.policyId ?? "").trim();
    if (!policyId) {
      return;
    }

    this.router.navigate(["/insurance", policyId]);
  }

  openInsuranceFromCreatePolicyId() {
    const policyId = this.createModel.policyId.trim();
    if (!policyId) {
      return;
    }

    this.router.navigate(["/insurance", policyId]);
  }

  onGridSortChanged(sortState: GridSortState[]) {
    this.gridSort = [...sortState];
    this.load();
  }

  private loadPolicyIdOptions() {
    this.insuranceFacade.listPlans().subscribe({
      next: (plans) => {
        this.insurancePlans = [...plans];
        this.policyIdOptions = [...new Set(plans.map((plan) => plan.policyId).filter((policyId) => !!policyId))].sort((a, b) => a.localeCompare(b));
        this.cdr.markForCheck();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.auth.logout();
          this.router.navigateByUrl("/login");
          return;
        }
        this.insurancePlans = [];
        this.policyIdOptions = [];
        this.cdr.markForCheck();
      },
    });
  }

}

