import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { NavigationEnd, Router } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";
import { ColDef } from "ag-grid-community";
import { AuthService } from "../../core/auth.service";
import { UsersFacade } from "../../features/users/application/users.facade";
import { UserAuditLogItem, UserItem } from "../../features/users/domain/user.models";
import { filter, finalize, Subscription } from "rxjs";
import { Modal } from "bootstrap";
import { DataGridComponent } from "../../shared/data-grid/data-grid.component";
import {
  UserFormDialogComponent,
  UserDialogData,
} from "../../pages/users/user-form-dialog/user-form-dialog.component";

@Component({
  selector: "app-user-list",
  standalone: true,
  imports: [
    CommonModule,
    DataGridComponent,
    UserFormDialogComponent,
  ],
  providers: [DatePipe],
  templateUrl: "../../pages/users/user-list/user-list.component.html",
  styleUrl: "../../pages/users/user-list/user-list.component.scss",
})
export class UserListComponent implements OnInit, OnDestroy {
  users: UserItem[] = [];
  listAccessAuditLogs: UserAuditLogItem[] = [];
  listAuditLoading = false;
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  createModalId = "create-user-modal";
  createDialogData: UserDialogData = { mode: "create", isAdmin: true };
  private readonly subscriptions = new Subscription();
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  columnDefs: ColDef<UserItem>[] = [
    { field: "id", headerName: "ID", width: 260, minWidth: 240, sort: "desc", sortIndex: 1 },
    { field: "name", headerName: "Name", width: 190, minWidth: 170 },
    { field: "email", headerName: "Email", width: 260, minWidth: 220 },
    { field: "role", headerName: "Role", width: 120 },
    {
      field: "createdAt",
      headerName: "Created",
      width: 160,
      sort: "desc",
      sortIndex: 0,
      valueFormatter: (params) => this.datePipe.transform(params.value, "MMM d, y") ?? "",
    },
    {
      field: "updatedAt",
      headerName: "Updated",
      width: 160,
      valueFormatter: (params) => this.datePipe.transform(params.value, "MMM d, y") ?? "",
    },
  ];

  defaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };

  constructor(
    private readonly usersFacade: UsersFacade,
    private readonly router: Router,
    public readonly auth: AuthService,
    public readonly datePipe: DatePipe,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
    if (this.auth.isAdmin()) {
      this.loadListAccessAuditLogs();
    }
    this.subscriptions.add(
      this.router.events
        .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
        .subscribe((event) => {
          if (event.urlAfterRedirects.startsWith("/users")) {
            this.load();
          }
        })
    );
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
    this.clearAlertTimer();
    this.cleanupModalArtifacts();
  }

  load() {
    this.loading = true;
    this.usersFacade
      .list()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (users) => {
          this.users = [...users];
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("Failed to load users.");
        },
      });

    if (this.auth.isAdmin()) {
      this.loadListAccessAuditLogs();
    }
  }

  openDetail(user?: UserItem | null) {
    if (!user) {
      return;
    }
    this.router.navigate(["/users", user.id]);
  }

  prepareCreate() {
    this.createDialogData = { mode: "create", isAdmin: true };
  }

  handleCreateSubmit(result: {
    name: string;
    email: string;
    role: "admin" | "user";
    password: string;
  }) {
    this.usersFacade
      .create({
        name: result.name,
        email: result.email,
        role: result.role,
        password: result.password,
      })
      .subscribe({
        next: () => {
          this.setAlert("User created.", "success");
          this.load();
          this.hideModal(this.createModalId);
        },
        error: () => {
          this.setAlert("Failed to create user.");
        },
      });
  }

  private loadListAccessAuditLogs() {
    this.listAuditLoading = true;
    this.usersFacade
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
