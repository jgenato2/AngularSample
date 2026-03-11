import { CommonModule, CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, Input, inject } from "@angular/core";
import { InsuranceFinancialAnalyticsItem } from "../../../../../features/insurance/domain/insurance.models";

@Component({
  selector: "app-stress-test-card",
  standalone: true,
  imports: [CommonModule],
  providers: [CurrencyPipe, DecimalPipe],
  templateUrl: "./stress-test-card.component.html",
  styleUrl: "./stress-test-card.component.scss",
})
export class StressTestCardComponent {
  private readonly currencyPipe = inject(CurrencyPipe);
  private readonly decimalPipe = inject(DecimalPipe);

  @Input() financial: InsuranceFinancialAnalyticsItem | null = null;


  constructor() {}

  formatCurrency(value: number | null | undefined, digits: string = "1.2-2") {
    const amount = value ?? 0;
    const formatted = this.currencyPipe.transform(Math.abs(amount), "USD", "symbol", digits) ?? "$0.00";
    return amount < 0 ? `(${formatted})` : formatted;
  }

  formatPercent(value: number | null | undefined, digits: string = "1.1-1") {
    const amount = value ?? 0;
    const formatted = this.decimalPipe.transform(Math.abs(amount), digits) ?? "0.0";
    return amount < 0 ? `(${formatted}%)` : `${formatted}%`;
  }

  signedValueClass(value: number | null | undefined) {
    return (value ?? 0) < 0 ? "text-danger" : "";
  }
}

