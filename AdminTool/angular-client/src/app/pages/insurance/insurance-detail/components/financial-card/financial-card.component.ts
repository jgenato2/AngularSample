import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, Input, inject } from "@angular/core";
import { InsurancePlanItem } from "../../../../../features/insurance/domain/insurance.models";

@Component({
  selector: "app-financial-card",
  standalone: true,
  imports: [CommonModule],
  providers: [CurrencyPipe],
  templateUrl: "./financial-card.component.html",
  styleUrl: "./financial-card.component.scss",
})
export class FinancialCardComponent {
  readonly currencyPipe = inject(CurrencyPipe);

  @Input({ required: true }) plan!: InsurancePlanItem;


  constructor() {}
}

