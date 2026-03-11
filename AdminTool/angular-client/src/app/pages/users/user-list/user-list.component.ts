import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { NavigationEnd, Router } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";
import { ColDef } from "ag-grid-community";
import { AuthService } from "../../../core/auth.service";
import { UsersFacade } from "../../../features/users/application/users.facade";
import { UserItem } from "../../../features/users/domain/user.models";
import { filter, finalize, Subscription } from "rxjs";
import { Modal } from "bootstrap";
import { DataGridComponent, GridSortState } from "../../../shared/data-grid/data-grid.component";
import { SearchQueryComponent } from "../../../shared/search-query/search-query.component";
import {
  UserFormDialogComponent,
  UserDialogData,
} from "../user-form-dialog/user-form-dialog.component";

@Component({
  selector: "app-user-list",
  standalone: true,
  imports: [
    CommonModule,
    DataGridComponent,
    SearchQueryComponent,
    UserFormDialogComponent,
  ],
  providers: [DatePipe],
  templateUrl: "./user-list.component.html",
  styleUrl: "./user-list.component.scss",
})
export class UserListComponent implements OnInit, OnDestroy {
  private readonly usersFacade = inject(UsersFacade);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  readonly datePipe = inject(DatePipe);
  private readonly cdr = inject(ChangeDetectorRef);

  users: UserItem[] = [];
  gridSearchQuery = "";
  gridSort: GridSortState[] = [];
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  createModalId = "create-user-modal";
  createDialogData: UserDialogData = { mode: "create", isAdmin: true };
  private readonly subscriptions = new Subscription();
  private listRequestSub: Subscription | null = null;
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;
  private searchDebounceId: ReturnType<typeof setTimeout> | null = null;

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


  constructor() {}

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
    this.listRequestSub?.unsubscribe();
    this.subscriptions.unsubscribe();
    this.clearAlertTimer();
    this.clearSearchDebounce();
    this.cleanupModalArtifacts();
  }

  load() {
    this.loading = true;
    this.listRequestSub?.unsubscribe();
    this.listRequestSub = this.usersFacade
      .list(this.gridSort, this.gridSearchQuery)
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
  }

  openDetail(user?: UserItem | null) {
    if (!user) {
      return;
    }
    this.router.navigate(["/users", user.id]);
  }

  onGridSortChanged(sortState: GridSortState[]) {
    this.gridSort = [...sortState];
    this.load();
  }

  onSearchQueryChange(value: string) {
    this.gridSearchQuery = value;
    this.scheduleSearchLoad();
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

  private scheduleSearchLoad() {
    this.clearSearchDebounce();
    this.searchDebounceId = setTimeout(() => {
      this.load();
      this.searchDebounceId = null;
    }, 250);
  }

  private clearSearchDebounce() {
    if (!this.searchDebounceId) {
      return;
    }

    clearTimeout(this.searchDebounceId);
    this.searchDebounceId = null;
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

