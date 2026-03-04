import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { AgGridModule } from "ag-grid-angular";
import { AllCommunityModule, ColDef, GridApi, GridReadyEvent } from "ag-grid-community";
import { finalize } from "rxjs";
import { AuthService } from "../../core/auth.service";
import { InsuranceFacade } from "../../features/insurance/application/insurance.facade";
import { InsuranceAuditLogItem, InsurancePlanItem } from "../../features/insurance/domain/insurance.models";
import { AuditLogListComponent } from "../../shared/audit-log-list/audit-log-list.component";

const DEFAULT_CREATE_STATUS_OPTIONS = ["New"];

@Component({
  selector: "app-insurance-list",
  standalone: true,
  imports: [CommonModule, FormsModule, AgGridModule, AuditLogListComponent],
  providers: [CurrencyPipe, DatePipe],
  templateUrl: "./insurance-list.component.html",
  styleUrl: "./insurance-list.component.scss",
})
export class InsuranceListComponent implements OnInit, OnDestroy, AfterViewInit {
  createStatusOptions = [...DEFAULT_CREATE_STATUS_OPTIONS];
  plans: InsurancePlanItem[] = [];
  listAccessAuditLogs: InsuranceAuditLogItem[] = [];
  loading = false;
  listAuditLoading = false;
  creating = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  modules = [AllCommunityModule];
  private gridApi: GridApi | null = null;
  @ViewChild("gridShell") private gridShellRef?: ElementRef<HTMLElement>;
  private readonly wheelHandler = (event: WheelEvent) => this.handleGridWheel(event);
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

  constructor(
    private readonly insuranceFacade: InsuranceFacade,
    public readonly auth: AuthService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    public readonly currencyPipe: CurrencyPipe,
    public readonly datePipe: DatePipe,
  ) {}

  ngOnInit() {
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

    this.insuranceFacade
      .listPlans()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (plans) => {
          this.plans = [...plans];
          if (this.gridApi) {
            this.gridApi.setGridOption("rowData", this.plans);
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

    if (this.auth.isAdmin()) {
      this.loadListAccessAuditLogs();
    }
  }


  onGridReady(event: GridReadyEvent) {
    this.gridApi = event.api;
    this.gridApi.setGridOption("rowData", this.plans);
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

  private loadListAccessAuditLogs() {
    this.listAuditLoading = true;
    this.insuranceFacade
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

  openDetail(plan?: InsurancePlanItem | null) {
    if (!plan) {
      return;
    }
    this.router.navigate(["/insurance", plan.policyId]);
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
}
