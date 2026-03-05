import { CommonModule } from "@angular/common";
import { Component, Input } from "@angular/core";

export interface SummaryStatItem {
  label: string;
  value: string | number;
  tone?: "default" | "primary" | "success" | "warning" | "danger";
}

@Component({
  selector: "app-summary-stats",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./summary-stats.component.html",
  styleUrl: "./summary-stats.component.scss",
})
export class SummaryStatsComponent {
  @Input() items: SummaryStatItem[] = [];

  cardToneClass(item: SummaryStatItem) {
    switch (item.tone) {
      case "primary":
        return "stat-primary";
      case "success":
        return "stat-success";
      case "warning":
        return "stat-warning";
      case "danger":
        return "stat-danger";
      default:
        return "stat-primary";
    }
  }
}
