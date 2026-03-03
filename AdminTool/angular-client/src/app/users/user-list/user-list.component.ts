import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { NavigationEnd, Router } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";
import { AgGridModule } from "ag-grid-angular";
import { AllCommunityModule, ColDef, GridApi, GridReadyEvent } from "ag-grid-community";
import { AuthService } from "../../core/auth.service";
import { UsersFacade } from "../../features/users/application/users.facade";
import { UserAuditLogItem, UserItem } from "../../features/users/domain/user.models";
import { filter, finalize, Subscription } from "rxjs";
import { Modal } from "bootstrap";
import { AuditLogListComponent } from "../../shared/audit-log-list/audit-log-list.component";
import {
  UserFormDialogComponent,
  UserDialogData,
} from "../user-form-dialog/user-form-dialog.component";

@Component({
  selector: "app-user-list",
  standalone: true,
  imports: [
    CommonModule,
    AgGridModule,
    AuditLogListComponent,
    UserFormDialogComponent,
  ],
  providers: [DatePipe],
  templateUrl: "./user-list.component.html",
  styleUrl: "./user-list.component.scss",
})
export class UserListComponent implements OnInit, OnDestroy, AfterViewInit {
  users: UserItem[] = [];
  listAccessAuditLogs: UserAuditLogItem[] = [];
  listAuditLoading = false;
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  modules = [AllCommunityModule];
  createModalId = "create-user-modal";
  createDialogData: UserDialogData = { mode: "create", isAdmin: true };
  private readonly subscriptions = new Subscription();
  private gridApi: GridApi | null = null;
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;
  @ViewChild("gridShell") private gridShellRef?: ElementRef<HTMLElement>;
  private readonly wheelHandler = (event: WheelEvent) => this.handleGridWheel(event);

  columnDefs: ColDef<UserItem>[] = [
    { field: "id", headerName: "ID", width: 260, minWidth: 240 },
    { field: "name", headerName: "Name", width: 190, minWidth: 170 },
    { field: "email", headerName: "Email", width: 260, minWidth: 220 },
    { field: "role", headerName: "Role", width: 120 },
    {
      field: "createdAt",
      headerName: "Created",
      width: 160,
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
    this.gridShellRef?.nativeElement.removeEventListener("wheel", this.wheelHandler);
    this.subscriptions.unsubscribe();
    this.clearAlertTimer();
    this.cleanupModalArtifacts();
  }

  ngAfterViewInit() {
    this.gridShellRef?.nativeElement.addEventListener("wheel", this.wheelHandler, { passive: false });
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
          if (this.gridApi) {
            this.gridApi.setGridOption("rowData", this.users);
          }
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

  onGridReady(event: GridReadyEvent) {
    this.gridApi = event.api;
    this.gridApi.setGridOption("rowData", this.users);
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
