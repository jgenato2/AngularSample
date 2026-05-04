import { Routes } from "@angular/router";
import { authGuard } from "./core/auth.guard";

export const routes: Routes = [
  { path: "", pathMatch: "full", redirectTo: "insurance" },
  {
    path: "login",
    loadComponent: () => import("./auth/login/login.component").then((m) => m.LoginComponent),
  },
  {
    path: "users",
    loadChildren: () => import("./pages/users/users.routes").then((m) => m.USERS_ROUTES),
  },
  {
    path: "insurance",
    loadChildren: () => import("./pages/insurance/insurance.routes").then((m) => m.INSURANCE_ROUTES),
  },
  {
    path: "providers",
    loadChildren: () => import("./pages/providers/providers.routes").then((m) => m.PROVIDERS_ROUTES),
  },
  {
    path: "claims",
    loadChildren: () => import("./pages/claims/claims.routes").then((m) => m.CLAIMS_ROUTES),
  },
  {
    path: "audit-logs",
    loadComponent: () => import("./pages/audit-log/audit-log.component").then((m) => m.AuditLogComponent),
    canActivate: [authGuard],
  },
  { path: "**", redirectTo: "insurance" },
];
