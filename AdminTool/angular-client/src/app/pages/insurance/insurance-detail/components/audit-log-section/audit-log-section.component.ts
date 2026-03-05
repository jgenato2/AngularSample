import { CommonModule, DatePipe } from "@angular/common";
import { Component, Input } from "@angular/core";
import { DataGridColumn, DataGridComponent } from "../../../../../shared/data-grid/data-grid.component";
import { SearchQueryComponent } from "../../../../../shared/search-query/search-query.component";

export interface AuditLogListItem {
  occurredAtUtc: string;
  performedBy: string;
  action: string;
  field: string;
  oldValue?: string | null;
  newValue?: string | null;
}

@Component({
  selector: "app-audit-log-section",
  standalone: true,
  imports: [CommonModule, DataGridComponent, SearchQueryComponent],
  providers: [DatePipe],
  templateUrl: "./audit-log-section.component.html",
  styleUrl: "./audit-log-section.component.scss",
})
export class AuditLogSectionComponent {
  @Input() items: AuditLogListItem[] = [];
  @Input() loading = false;
  searchQuery = "";

  readonly columns: DataGridColumn[] = [
    { key: "occurredAtUtc", label: "Time (UTC)", type: "dateTime", minWidth: 190, flex: 1.2 },
    { key: "performedBy", label: "Actor", minWidth: 150, flex: 1 },
    { key: "action", label: "Action", minWidth: 140, flex: 1 },
    { key: "field", label: "Field", minWidth: 140, flex: 1 },
    { key: "oldValue", label: "Old", minWidth: 160, flex: 1 },
    { key: "newValue", label: "New", minWidth: 160, flex: 1 },
  ];

  constructor(public readonly datePipe: DatePipe) {}

  get rows() {
    return this.items.map((item) => ({
      occurredAtUtc: item.occurredAtUtc,
      performedBy: item.performedBy,
      action: item.action,
      field: item.field,
      oldValue: item.oldValue || "-",
      newValue: item.newValue || "-",
    }));
  }
}

