import { CommonModule, DatePipe } from "@angular/common";
import { Component, Input } from "@angular/core";
import { InsurancePlanItem } from "../../../../features/insurance/domain/insurance.models";

@Component({
  selector: "app-timeline-card",
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: "./timeline-card.component.html",
  styleUrl: "./timeline-card.component.scss",
})
export class TimelineCardComponent {
  @Input({ required: true }) plan!: InsurancePlanItem;

  constructor(public readonly datePipe: DatePipe) {}
}
