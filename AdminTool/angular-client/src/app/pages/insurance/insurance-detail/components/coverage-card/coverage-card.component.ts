import { CommonModule } from "@angular/common";
import { Component, Input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { InsurancePlanItem } from "../../../../../features/insurance/domain/insurance.models";

@Component({
  selector: "app-coverage-card",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./coverage-card.component.html",
  styleUrl: "./coverage-card.component.scss",
})
export class CoverageCardComponent {
  @Input({ required: true }) plan!: InsurancePlanItem;
  @Input() memberClaimId: string | null = null;

  policyStatusClass(status: string | null | undefined) {
    const value = (status ?? "").trim().toLowerCase();
    switch (value) {
      case "new":
        return "text-secondary";
      case "underwriting":
        return "text-info";
      case "pending activation":
        return "text-primary";
      case "active":
      case "renewed":
        return "text-success";
      case "grace period":
      case "pending renewal":
        return "text-warning";
      case "suspended":
        return "text-dark";
      case "cancelled":
      case "expired":
        return "text-danger";
      default:
        return "text-body-secondary";
    }
  }
}

