import { CommonModule, DatePipe } from "@angular/common";
import { Component, Input } from "@angular/core";

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
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: "./audit-log-section.component.html",
  styleUrl: "./audit-log-section.component.scss",
})
export class AuditLogSectionComponent {
  @Input() items: AuditLogListItem[] = [];
  @Input() loading = false;

  constructor(public readonly datePipe: DatePipe) {}
}
