import { Routes } from "@angular/router";
import { authGuard } from "../../core/auth.guard";

export const CLAIMS_ROUTES: Routes = [
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () => import("./claim-list/claim-list.component").then((m) => m.ClaimListComponent),
  },
  {
    path: ":claimId",
    canActivate: [authGuard],
    loadComponent: () => import("./claim-detail/claim-detail.component").then((m) => m.ClaimDetailComponent),
  },
];
