import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, OnDestroy, Output } from "@angular/core";

@Component({
  selector: "app-search-query",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./search-query.component.html",
  styleUrl: "./search-query.component.scss",
})
export class SearchQueryComponent implements OnDestroy {
  @Input() query = "";
  @Input() placeholder = "Search";
  @Input() showOpen = false;
  @Input() showRefresh = false;
  @Input() openDisabled = false;
  @Input() refreshDisabled = false;
  @Input() matches: unknown[] = [];
  @Input() matchIdentity: ((match: unknown) => string) | null = null;

  @Output() queryChange = new EventEmitter<string>();
  @Output() openClicked = new EventEmitter<void>();
  @Output() matchOpened = new EventEmitter<unknown>();
  @Output() refreshClicked = new EventEmitter<void>();

  alertMessage: string | null = null;
  private alertTimerId: ReturnType<typeof setTimeout> | null = null;

  ngOnDestroy() {
    this.clearAlert();
  }

  get isOpenDisabled() {
    return this.openDisabled || !this.query.trim() || this.matches.length !== 1;
  }

  onInput(event: Event) {
    const target = event.target as HTMLInputElement | null;
    this.clearAlert();
    this.queryChange.emit(target?.value ?? "");
  }

  clear() {
    this.clearAlert();
    this.queryChange.emit("");
  }

  open() {
    this.clearAlert();
    this.openClicked.emit();

    const query = this.query.trim().toLowerCase();
    if (!query || this.isOpenDisabled) {
      return;
    }

    if (this.matches.length === 0) {
      this.setAlert("No record matched your search.");
      return;
    }

    const identity = this.matchIdentity;
    if (identity) {
      const exactMatch = this.matches.find((match) => identity(match).trim().toLowerCase() === query);
      if (exactMatch) {
        this.matchOpened.emit(exactMatch);
        return;
      }
    }

    if (this.matches.length === 1) {
      this.matchOpened.emit(this.matches[0]);
      return;
    }

    this.setAlert("Multiple records matched. Refine your search.");
  }

  refresh() {
    this.refreshClicked.emit();
  }

  private setAlert(message: string) {
    this.clearAlert();
    this.alertMessage = message;
    this.alertTimerId = setTimeout(() => {
      this.alertMessage = null;
      this.alertTimerId = null;
    }, 3500);
  }

  private clearAlert() {
    if (this.alertTimerId) {
      clearTimeout(this.alertTimerId);
      this.alertTimerId = null;
    }
    this.alertMessage = null;
  }
}
