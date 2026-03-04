import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { AuthService } from "../../../core/auth.service";
import { UsersFacade } from "../../../features/users/application/users.facade";
import { UserAuditLogItem, UserItem } from "../../../features/users/domain/user.models";
import { HttpErrorResponse } from "@angular/common/http";
import { finalize } from "rxjs";
import { Modal } from "bootstrap";
import { ColDef } from "ag-grid-community";
import {
  UserFormDialogComponent,
  UserDialogData,
} from "../user-form-dialog/user-form-dialog.component";
import { DataGridComponent } from "../../../shared/data-grid/data-grid.component";

@Component({
  selector: "app-user-detail",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    UserFormDialogComponent,
    DataGridComponent,
  ],
  providers: [DatePipe],
  templateUrl: "./user-detail.component.html",
  styleUrl: "./user-detail.component.scss",
})
export class UserDetailComponent implements OnInit, OnDestroy {
  user: UserItem | null = null;
  auditRows: UserAuditLogItem[] = [];
  loading = false;
  loadingAudit = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  editModalId = "edit-user-modal";
  editDialogData: UserDialogData = { mode: "edit", isAdmin: false };
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
    { field: "action", headerName: "Action", width: 180, minWidth: 160 },
    { field: "field", headerName: "Field", width: 180, minWidth: 160 },
    { field: "oldValue", headerName: "Old Value", width: 220, minWidth: 180 },
    { field: "newValue", headerName: "New Value", width: 220, minWidth: 180 },
  ];

  constructor(
    private readonly route: ActivatedRoute,
    private readonly usersFacade: UsersFacade,
    private readonly router: Router,
    public readonly auth: AuthService,
    public readonly datePipe: DatePipe,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
  }

  ngOnDestroy() {
    this.clearAlertTimer();
  }

  get canEdit() {
    const current = this.auth.user();
    return !!this.user && !!current && (current.role === "admin" || current.id === this.user.id);
  }

  load() {
    const id = this.route.snapshot.paramMap.get("id");
    if (!id) {
      return;
    }

    this.loading = true;
    this.usersFacade
      .getById(id)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (user) => {
          this.user = user;
          this.loadAuditLogs(user.id);
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("User not found.");
          this.router.navigateByUrl("/users");
        },
      });
  }

  loadAuditLogs(id: string) {
    this.loadingAudit = true;
    this.auditRows = [];

    this.usersFacade
      .getAuditLogs(id)
      .pipe(finalize(() => {
        this.loadingAudit = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (items) => {
          this.auditRows = items;
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }

          this.auditRows = [];
          this.cdr.markForCheck();
        },
      });
  }

  openEdit() {
    if (!this.user) {
      return;
    }
    this.editDialogData = {
      mode: "edit",
      user: this.user,
      isAdmin: this.auth.isAdmin(),
    };
  }

  handleEditSubmit(result: {
    name: string;
    email: string;
    role?: "admin" | "user";
    password?: string;
  }) {
    if (!this.user) {
      return;
    }

    this.usersFacade
      .update(this.user.id, {
        name: result.name,
        email: result.email,
        role: this.auth.isAdmin() ? result.role : undefined,
        password: result.password || undefined,
      })
      .subscribe({
        next: (user) => {
          queueMicrotask(() => {
            this.user = user;
            this.cdr.detectChanges();
          });
          this.setAlert("User updated.", "success");
          this.hideModal(this.editModalId);
        },
        error: () => {
          this.setAlert("Failed to update user.");
        },
      });
  }

  remove() {
    if (!this.user) {
      return;
    }
    if (!confirm(`Delete ${this.user.name}?`)) {
      return;
    }

    this.usersFacade.remove(this.user.id).subscribe({
      next: () => {
        this.setAlert("User deleted.", "success");
        this.router.navigateByUrl("/users");
      },
      error: () => {
        this.setAlert("Failed to delete user.");
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

  private hideModal(id: string) {
    const element = document.getElementById(id);
    if (!element) {
      this.cleanupModalArtifacts();
      return;
    }
    const modal = Modal.getOrCreateInstance(element);
    modal?.hide();
    window.setTimeout(() => this.cleanupModalArtifacts(), 250);
  }

  private cleanupModalArtifacts() {
    document.body.classList.remove("modal-open");
    document.body.style.removeProperty("padding-right");

    const backdrops = document.querySelectorAll(".modal-backdrop");
    backdrops.forEach((backdrop) => backdrop.remove());
  }
}

