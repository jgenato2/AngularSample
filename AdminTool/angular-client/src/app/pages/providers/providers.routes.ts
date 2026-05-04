import { Routes } from "@angular/router";
import { authGuard } from "../../core/auth.guard";

export const PROVIDERS_ROUTES: Routes = [
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () => import("./provider-list/provider-list.component").then((m) => m.ProviderListComponent),
  },
  {
    path: ":provider",
    canActivate: [authGuard],
    loadComponent: () => import("./provider-detail/provider-detail.component").then((m) => m.ProviderDetailComponent),
  },
];
