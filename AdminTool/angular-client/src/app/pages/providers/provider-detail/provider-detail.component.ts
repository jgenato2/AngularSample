import { CommonModule, DatePipe } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, ParamMap, Router } from "@angular/router";
import { DetailActionsBarComponent } from '../../../shared/detail-actions/detail-actions-bar.component';
import { finalize, Subscription, timeout } from "rxjs";
import { AuthService } from "../../../core/auth.service";
import { SummaryStatItem, SummaryStatsComponent } from "../../../shared/summary-stats/summary-stats.component";
import { ProviderDetailItem, ProvidersService } from "../providers.service";

@Component({
  selector: "app-provider-detail",
  standalone: true,
  imports: [CommonModule, FormsModule, SummaryStatsComponent, DetailActionsBarComponent],
  providers: [DatePipe],
  templateUrl: "./provider-detail.component.html",
  styleUrls: ["./provider-detail.component.scss"],
})
export class ProviderDetailComponent implements OnInit, OnDestroy {
  provider: ProviderDetailItem | null = null;
  loading = false;
  noteDraft = "";
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";

  private readonly subscriptions = new Subscription();
  private loadRequestSub: Subscription | null = null;
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  constructor(
    private readonly providersService: ProvidersService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    public readonly auth: AuthService,
    public readonly datePipe: DatePipe,
  ) {}

  ngOnInit() {
    this.subscriptions.add(
      this.route.paramMap.subscribe((params) => {
        this.loadFromRoute(params);
      })
    );
  }

  ngOnDestroy() {
    this.loadRequestSub?.unsubscribe();
    this.subscriptions.unsubscribe();
    this.clearAlertTimer();
  }

  goBack() {
    this.router.navigate(["/providers"]);
  }

  viewPlans() {
    if (!this.provider?.provider) {
      return;
    }

    this.router.navigate(["/insurance"], { queryParams: { query: this.provider.provider } });
  }

  get summaryItems(): SummaryStatItem[] {
    if (!this.provider) {
      return [];
    }

    return [
      { label: "Plan count", value: this.provider.planCount },
      { label: "Active plans", value: this.provider.activePlans, tone: "success" },
      { label: "Pending plans", value: this.provider.pendingPlans, tone: "warning" },
      { label: "Expired plans", value: this.provider.expiredPlans, tone: "danger" },
    ];
  }

  private loadFromRoute(params: ParamMap) {
    const providerName = (params.get("provider") ?? "").trim();
    if (!providerName) {
      this.provider = null;
      this.setAlert("Provider is required.");
      return;
    }

    this.loading = true;
    this.provider = null;
    this.noteDraft = "";
    this.alertMessage = null;
    this.loadRequestSub?.unsubscribe();
    this.cdr.markForCheck();

    this.loadRequestSub = this.providersService
      .getByProvider(providerName)
      .pipe(timeout(15000))
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.provider = response.item;
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            this.cdr.markForCheck();
            return;
          }

          if (error.status === 404) {
            this.setAlert("Provider not found.");
            return;
          }

          this.setAlert("Failed to load provider details.");
        },
      });
  }

  private setAlert(message: string, type: "danger" | "success" = "danger") {
    this.clearAlertTimer();
    this.alertMessage = message;
    this.alertType = type;
    this.cdr.markForCheck();
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
