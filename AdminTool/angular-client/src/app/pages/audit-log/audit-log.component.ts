import { CommonModule } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { ColDef } from "ag-grid-community";
import { finalize } from "rxjs";
import { AuthService } from "../../core/auth.service";
import { AuditLogFacade, AuditLogListItem } from "../../features/audit-log/application/audit-log.facade";
import { DataGridComponent } from "../../shared/data-grid/data-grid.component";
import { AuditLogSectionComponent } from '../insurance/insurance-detail/components/audit-log-section/audit-log-section.component';

@Component({
  selector: "app-audit-log",
  standalone: true,
  imports: [CommonModule, DataGridComponent, AuditLogSectionComponent],
  templateUrl: "./audit-log.component.html",
  styleUrl: "./audit-log.component.scss",
})
export class AuditLogComponent implements OnInit, OnDestroy {
  auditRows: AuditLogListItem[] = [];
  loading = false;
  page = 1;
  readonly pageSize = 25;
  totalItems = 0;
  totalPages = 1;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  readonly auditColumnDefs: ColDef[] = [
    {
      field: "occurredAtUtc",
      headerName: "Time (UTC)",
      width: 220,
      minWidth: 190,
      sort: "desc",
      sortIndex: 0,
      valueFormatter: (params) => {
        const value = params.value;
        if (!value) {
          return "";
        }
        return new Intl.DateTimeFormat("en-US", {
          month: "short",
          day: "numeric",
          year: "numeric",
          hour: "numeric",
          minute: "2-digit",
        }).format(new Date(value));
      },
    },
    { field: "performedBy", headerName: "Actor", width: 190, minWidth: 170 },
    { field: "entityId", headerName: "Record", width: 170, minWidth: 150 },
    { field: "action", headerName: "Action", width: 180, minWidth: 160 },
    { field: "field", headerName: "Field", width: 180, minWidth: 160 },
    {
      field: "changeSummary",
      headerName: "Change",
      minWidth: 320,
      flex: 1.4,
      valueGetter: (params) => this.buildChangeSummary(params.data as AuditLogListItem | null),
    },
  ];

  constructor(
    private readonly auditLogFacade: AuditLogFacade,
    private readonly router: Router,
    public readonly auth: AuthService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.load(1);
  }

  ngOnDestroy() {
    this.clearAlertTimer();
  }

  get canGoPrevious() {
    return this.page > 1;
  }

  get canGoNext() {
    return this.page < this.totalPages;
  }

  goToFirstPage() {
    if (!this.canGoPrevious || this.loading) {
      return;
    }

    this.load(1);
  }

  goToPreviousPage() {
    if (!this.canGoPrevious || this.loading) {
      return;
    }

    this.load(this.page - 1);
  }

  goToNextPage() {
    if (!this.canGoNext || this.loading) {
      return;
    }

    this.load(this.page + 1);
  }

  goToLastPage() {
    if (!this.canGoNext || this.loading) {
      return;
    }

    this.load(this.totalPages);
  }

  load(requestedPage = this.page) {
    if (!this.auth.isAdmin()) {
      this.auditRows = [];
      this.page = 1;
      this.totalPages = 1;
      this.totalItems = 0;
      this.cdr.markForCheck();
      return;
    }

    this.loading = true;

    this.auditLogFacade
      .getAllListAccessAuditLogs(requestedPage, this.pageSize)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (result) => {
          this.auditRows = result.items;
          this.page = result.pagination.page;
          this.totalPages = result.pagination.totalPages;
          this.totalItems = result.pagination.totalItems;
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }

            this.auditRows = [];
            this.page = 1;
            this.totalPages = 1;
            this.totalItems = 0;
            this.setAlert("Failed to load audit logs.");
            this.cdr.markForCheck();
        },
      });
  }

  private buildChangeSummary(item: AuditLogListItem | null | undefined) {
    if (!item) {
      return "";
    }

    const hasOldValue = !!item.oldValue?.trim();
    const hasNewValue = !!item.newValue?.trim();

    if (item.action.toLowerCase() === "viewed") {
      return `Viewed ${item.field} on record '${item.entityId}'.`;
    }

    if (!hasOldValue && hasNewValue) {
      return `${item.action} ${item.field} on '${item.entityId}': set to '${item.newValue}'.`;
    }

    if (hasOldValue && !hasNewValue) {
      return `${item.action} ${item.field} on '${item.entityId}': removed '${item.oldValue}'.`;
    }

    if (hasOldValue && hasNewValue) {
      return `${item.action} ${item.field} on '${item.entityId}': '${item.oldValue}' → '${item.newValue}'.`;
    }

    return `${item.action} ${item.field} on '${item.entityId}'.`;
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
