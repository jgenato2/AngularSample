import { CommonModule, DatePipe } from "@angular/common";
import { Component, Input, inject } from "@angular/core";
import { DataGridComponent } from "../../../../../shared/data-grid/data-grid.component";
import { SearchQueryComponent } from "../../../../../shared/search-query/search-query.component";

// Match the full interface used by the caller
export interface AuditLogListItem {
  id: string;
  entityId: string;
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
  readonly datePipe = inject(DatePipe);

  @Input() items: AuditLogListItem[] = [];
  @Input() loading = false;
  searchQuery = "";


  columnDefs = [
    { field: "occurredAtUtc", headerName: "Time (UTC)", minWidth: 190, flex: 1.2, valueFormatter: (params: { value: string }) => this.datePipe.transform(params.value, 'MMM d, y, HH:mm') ?? '' },
    { field: "performedBy", headerName: "Actor", minWidth: 150, flex: 1 },
    { field: "entityId", headerName: "Record", minWidth: 150, flex: 1 },
    { field: "action", headerName: "Action", minWidth: 140, flex: 1 },
    { field: "field", headerName: "Field", minWidth: 140, flex: 1 },
    { field: "oldValue", headerName: "Old", minWidth: 160, flex: 1 },
    { field: "newValue", headerName: "New", minWidth: 160, flex: 1 },
  ];

  defaultColDef = {
    sortable: true,
    filter: true,
    resizable: true,
  };


  constructor() {}

  get users() {
    // Ensure all required fields are present for ag-Grid and map entityId/id if needed
    return this.items.map((item) => ({
      id: item.id,
      entityId: item.entityId,
      occurredAtUtc: item.occurredAtUtc,
      performedBy: item.performedBy,
      action: item.action,
      field: item.field,
      oldValue: item.oldValue ?? '-',
      newValue: item.newValue ?? '-',
    }));
  }
}

