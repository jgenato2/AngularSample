import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, Output } from "@angular/core";

@Component({
  selector: "app-search-query",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./search-query.component.html",
  styleUrl: "./search-query.component.scss",
})
export class SearchQueryComponent {
  @Input() query = "";
  @Input() placeholder = "Search";
  @Input() showOpen = false;
  @Input() showRefresh = false;
  @Input() openDisabled = false;
  @Input() refreshDisabled = false;

  @Output() queryChange = new EventEmitter<string>();
  @Output() openClicked = new EventEmitter<void>();
  @Output() refreshClicked = new EventEmitter<void>();

  onInput(event: Event) {
    const target = event.target as HTMLInputElement | null;
    this.queryChange.emit(target?.value ?? "");
  }

  clear() {
    this.queryChange.emit("");
  }

  open() {
    this.openClicked.emit();
  }

  refresh() {
    this.refreshClicked.emit();
  }
}
