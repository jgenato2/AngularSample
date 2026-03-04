import { CommonModule, DecimalPipe } from "@angular/common";
import { Component, Input } from "@angular/core";
import { InsuranceFinancialAnalyticsItem } from "../../../../../features/insurance/domain/insurance.models";

@Component({
  selector: "app-risk-model-card",
  standalone: true,
  imports: [CommonModule],
  providers: [DecimalPipe],
  templateUrl: "./risk-model-card.component.html",
  styleUrl: "./risk-model-card.component.scss",
})
export class RiskModelCardComponent {
  @Input() financial: InsuranceFinancialAnalyticsItem | null = null;

  constructor(public readonly decimalPipe: DecimalPipe) {}

  formatPercent(value: number | null | undefined, digits: string = "1.1-1") {
    const amount = value ?? 0;
    const formatted = this.decimalPipe.transform(Math.abs(amount), digits) ?? "0.0";
    return amount < 0 ? `(${formatted}%)` : `${formatted}%`;
  }

  signedValueClass(value: number | null | undefined) {
    return (value ?? 0) < 0 ? "text-danger" : "";
  }

  riskBandClass(riskBand: string | null | undefined) {
    const value = (riskBand ?? "").toLowerCase();
    if (value === "low") {
      return "text-success";
    }

    if (value === "moderate") {
      return "text-warning";
    }

    if (value === "high") {
      return "text-danger";
    }

    return "text-body-secondary";
  }
}

