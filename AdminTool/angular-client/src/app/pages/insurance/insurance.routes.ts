import { Routes } from "@angular/router";
import { authGuard } from "../../core/auth.guard";

export const INSURANCE_ROUTES: Routes = [
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () => import("./insurance-list/insurance-list.component").then((m) => m.InsuranceListComponent),
  },
  {
    path: ":policyId",
    canActivate: [authGuard],
    loadComponent: () => import("./insurance-detail/insurance-detail.component").then((m) => m.InsuranceDetailComponent),
  },
];
