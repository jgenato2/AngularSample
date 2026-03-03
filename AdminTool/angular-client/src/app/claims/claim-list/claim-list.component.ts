import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { Router, RouterLink } from "@angular/router";
import { AgGridModule } from "ag-grid-angular";
import { AllCommunityModule, CellClickedEvent, ColDef, GridApi, GridReadyEvent } from "ag-grid-community";
import { finalize } from "rxjs";
import { AuthService } from "../../core/auth.service";
import { ClaimsFacade } from "../../features/claims/application/claims.facade";
import { ClaimAuditLogItem, ClaimItem, ClaimStatusWorkflowItem, CreateClaimPayload } from "../../features/claims/domain/claim.models";
import { InsuranceFacade } from "../../features/insurance/application/insurance.facade";
import { AuditLogListComponent } from "../../shared/audit-log-list/audit-log-list.component";

const DEFAULT_CREATE_STATUS_OPTIONS = ["Submitted"];

@Component({
  selector: "app-claim-list",
  standalone: true,
  imports: [CommonModule, FormsModule, AgGridModule, RouterLink, AuditLogListComponent],
  providers: [CurrencyPipe, DatePipe],
  templateUrl: "./claim-list.component.html",
  styleUrl: "./claim-list.component.scss",
})
export class ClaimListComponent implements OnInit, OnDestroy, AfterViewInit {
  createStatusOptions = [...DEFAULT_CREATE_STATUS_OPTIONS];
  statusWorkflow: Record<string, string[]> = {};
  policyIdOptions: string[] = [];
  claims: ClaimItem[] = [];
  displayedClaims: ClaimItem[] = [];
  listAccessAuditLogs: ClaimAuditLogItem[] = [];
  policyIdSearch = "";
  loading = false;
  listAuditLoading = false;
  creating = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  modules = [AllCommunityModule];
  private gridApi: GridApi | null = null;
  @ViewChild("gridShell") private gridShellRef?: ElementRef<HTMLElement>;
  private readonly wheelHandler = (event: WheelEvent) => this.handleGridWheel(event);

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

  constructor(
    private readonly claimsFacade: ClaimsFacade,
    private readonly insuranceFacade: InsuranceFacade,
    public readonly auth: AuthService,
    public readonly currencyPipe: CurrencyPipe,
    public readonly datePipe: DatePipe,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.loadPolicyIdOptions();
    this.loadStatusWorkflow();
    this.load();
    if (this.auth.isAdmin()) {
      this.loadListAccessAuditLogs();
    }
  }

  ngAfterViewInit() {
    this.gridShellRef?.nativeElement.addEventListener("wheel", this.wheelHandler, { passive: false });
  }

  ngOnDestroy() {
    this.gridShellRef?.nativeElement.removeEventListener("wheel", this.wheelHandler);
  }

  columnDefs: ColDef<ClaimItem>[] = [
    { field: "claimId", headerName: "Claim ID", width: 150, minWidth: 140 },
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

  onGridReady(event: GridReadyEvent) {
    this.gridApi = event.api;
    this.gridApi.setGridOption("rowData", this.displayedClaims);
  }

  load() {
    this.loading = true;
    this.alertMessage = null;

    this.claimsFacade
      .list()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (items) => {
          this.claims = [...items];
          this.applyPolicyIdFilter();
          if (this.gridApi) {
            this.gridApi.setGridOption("rowData", this.displayedClaims);
          }
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

    if (this.auth.isAdmin()) {
      this.loadListAccessAuditLogs();
    }
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

  private loadListAccessAuditLogs() {
    this.listAuditLoading = true;
    this.claimsFacade
      .getListAccessAuditLogs()
      .pipe(finalize(() => {
        this.listAuditLoading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (items) => {
          this.listAccessAuditLogs = [...items];
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.listAccessAuditLogs = [];
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

  private handleGridWheel(event: WheelEvent) {
    const gridShell = event.currentTarget as HTMLElement | null;
    if (!gridShell) {
      return;
    }

    const horizontalViewport = gridShell.querySelector(".ag-body-horizontal-scroll-viewport") as HTMLElement | null;
    if (!horizontalViewport) {
      return;
    }

    if (horizontalViewport.scrollWidth <= horizontalViewport.clientWidth) {
      return;
    }

    const delta = Math.abs(event.deltaX) > 0 ? event.deltaX : event.deltaY;
    horizontalViewport.scrollLeft += delta;
    event.preventDefault();
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

  onPolicyIdSearchChange(value: string) {
    this.policyIdSearch = value;
    this.applyPolicyIdFilter();
    if (this.gridApi) {
      this.gridApi.setGridOption("rowData", this.displayedClaims);
    }
    this.cdr.markForCheck();
  }

  clearPolicyIdSearch() {
    this.onPolicyIdSearchChange("");
  }

  openInsuranceFromSearch() {
    const policyId = this.policyIdSearch.trim();
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

  private applyPolicyIdFilter() {
    const query = this.policyIdSearch.trim().toLowerCase();
    if (!query) {
      this.displayedClaims = [...this.claims];
      return;
    }

    this.displayedClaims = this.claims.filter((claim) => claim.policyId.toLowerCase().includes(query));
  }
}
