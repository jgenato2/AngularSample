import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, Input, inject } from "@angular/core";
import { InsuranceFinancialAnalyticsItem } from "../../../../../features/insurance/domain/insurance.models";

@Component({
  selector: "app-baseline-projection-card",
  standalone: true,
  imports: [CommonModule],
  providers: [CurrencyPipe],
  templateUrl: "./baseline-projection-card.component.html",
  styleUrl: "./baseline-projection-card.component.scss",
})
export class BaselineProjectionCardComponent {
  private readonly currencyPipe = inject(CurrencyPipe);

  @Input() financial: InsuranceFinancialAnalyticsItem | null = null;


  constructor() {}

  formatCurrency(value: number | null | undefined, digits: string = "1.2-2") {
    const amount = value ?? 0;
    const formatted = this.currencyPipe.transform(Math.abs(amount), "USD", "symbol", digits) ?? "$0.00";
    return amount < 0 ? `(${formatted})` : formatted;
  }

  signedValueClass(value: number | null | undefined) {
    return (value ?? 0) < 0 ? "text-danger" : "";
  }
}

