import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { NavigationEnd, Router } from "@angular/router";
import { ColDef } from "ag-grid-community";
import { filter, finalize, Subscription } from "rxjs";
import { AuthService } from "../../../core/auth.service";
import { DataGridComponent, GridSortState } from "../../../shared/data-grid/data-grid.component";
import { SearchQueryComponent } from "../../../shared/search-query/search-query.component";
import { ProviderItem, ProvidersService } from "../providers.service";

@Component({
  selector: "app-provider-list",
  standalone: true,
  imports: [CommonModule, DataGridComponent, SearchQueryComponent],
  providers: [DatePipe],
  templateUrl: "./provider-list.component.html",
  styleUrl: "./provider-list.component.scss",
})
export class ProviderListComponent implements OnInit, OnDestroy {
  private readonly providersService = inject(ProvidersService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly datePipe = inject(DatePipe);

  providers: ProviderItem[] = [];
  gridSearchQuery = "";
  gridSort: GridSortState[] = [];
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private readonly subscriptions = new Subscription();
  private listRequestSub: Subscription | null = null;
  private searchDebounceId: ReturnType<typeof setTimeout> | null = null;
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  columnDefs: ColDef<ProviderItem>[] = [
    { field: "provider", headerName: "Provider", minWidth: 240, sort: "asc", sortIndex: 0 },
    { field: "planCount", headerName: "Plans", width: 140, minWidth: 120 },
    {
      field: "latestEffectiveDate",
      headerName: "Latest Effective",
      width: 190,
      minWidth: 170,
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
          if (event.urlAfterRedirects.startsWith("/providers")) {
            this.load();
          }
        })
    );
  }

  ngOnDestroy() {
    this.listRequestSub?.unsubscribe();
    this.subscriptions.unsubscribe();
    this.clearSearchDebounce();
    this.clearAlertTimer();
  }

  load() {
    this.loading = true;
    this.listRequestSub?.unsubscribe();

    this.listRequestSub = this.providersService
      .list(this.gridSort, this.gridSearchQuery)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.providers = [...(response.items ?? [])];
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("Failed to load providers.");
        },
      });
  }

  onGridSortChanged(sortState: GridSortState[]) {
    this.gridSort = [...sortState];
    this.load();
  }

  openInsuranceForProvider(provider?: ProviderItem | null) {
    const providerName = provider?.provider?.trim();
    if (!providerName) {
      return;
    }

    this.router.navigate(["/providers", providerName]);
  }

  onSearchQueryChange(value: string) {
    this.gridSearchQuery = value;
    this.scheduleSearchLoad();
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
