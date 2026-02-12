import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { NavigationEnd, Router } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";
import { AgGridModule } from "ag-grid-angular";
import { AllCommunityModule, ColDef, GridApi, GridReadyEvent } from "ag-grid-community";
import { AuthService } from "../../core/auth.service";
import { UsersService, UserItem } from "../users.service";
import { filter, finalize, Subscription } from "rxjs";
import { Modal } from "bootstrap";
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
    UserFormDialogComponent,
  ],
  providers: [DatePipe],
  templateUrl: "./user-list.component.html",
  styleUrl: "./user-list.component.scss",
})
export class UserListComponent implements OnInit, OnDestroy {
  users: UserItem[] = [];
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  modules = [AllCommunityModule];
  createModalId = "create-user-modal";
  createDialogData: UserDialogData = { mode: "create", isAdmin: true };
  private readonly subscriptions = new Subscription();
  private gridApi: GridApi | null = null;

  columnDefs: ColDef<UserItem>[] = [
    { field: "name", headerName: "Name", flex: 1 },
    { field: "email", headerName: "Email", flex: 1.2 },
    { field: "role", headerName: "Role", width: 120 },
    {
      field: "createdAt",
      headerName: "Created",
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
    private readonly usersService: UsersService,
    private readonly router: Router,
    public readonly auth: AuthService,
    private readonly datePipe: DatePipe,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
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
  }

  load() {
    this.loading = true;
    this.usersService
      .list()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.users = [...response.items];
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
    this.usersService
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

  private setAlert(message: string, type: "danger" | "success" = "danger") {
    this.alertMessage = message;
    this.alertType = type;
  }

  private hideModal(id: string) {
    const element = document.getElementById(id);
    if (!element) {
      return;
    }
    const modal = Modal.getOrCreateInstance(element);
    modal?.hide();
  }
}
