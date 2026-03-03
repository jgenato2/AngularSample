import { CommonModule, DatePipe } from "@angular/common";
import { Component, Input } from "@angular/core";

export interface SharedAuditLogItem {
  occurredAtUtc: string;
  performedBy: string;
  action: string;
  field: string;
}

@Component({
  selector: "app-audit-log-list",
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: "./audit-log-list.component.html",
  styleUrl: "./audit-log-list.component.scss",
})
export class AuditLogListComponent {
  @Input() title = "List Access Audit";
  @Input() loading = false;
  @Input() emptyMessage = "No audit entries available.";
  @Input() items: SharedAuditLogItem[] = [];

  constructor(public readonly datePipe: DatePipe) {}
}
