import { Routes } from "@angular/router";
import { LoginComponent } from "./auth/login/login.component";
import { UserListComponent } from "./pages/users/user-list/user-list.component";
import { UserDetailComponent } from "./pages/users/user-detail/user-detail.component";
import { InsuranceListComponent } from "./pages/insurance/insurance-list/insurance-list.component";
import { InsuranceDetailComponent } from "./pages/insurance/insurance-detail/insurance-detail.component";
import { ClaimListComponent } from "./pages/claims/claim-list/claim-list.component";
import { ClaimDetailComponent } from "./pages/claims/claim-detail/claim-detail.component";
import { AuditLogComponent } from "./pages/audit-log/audit-log.component";
import { authGuard } from "./core/auth.guard";

export const routes: Routes = [
  { path: "", pathMatch: "full", redirectTo: "users" },
  { path: "login", component: LoginComponent },
  { path: "users", component: UserListComponent, canActivate: [authGuard] },
  { path: "users/:id", component: UserDetailComponent, canActivate: [authGuard] },
  { path: "insurance", component: InsuranceListComponent, canActivate: [authGuard] },
  { path: "insurance/:policyId", component: InsuranceDetailComponent, canActivate: [authGuard] },
  { path: "claims", component: ClaimListComponent, canActivate: [authGuard] },
  { path: "claims/:claimId", component: ClaimDetailComponent, canActivate: [authGuard] },
  { path: "audit-logs", component: AuditLogComponent, canActivate: [authGuard] },
  { path: "**", redirectTo: "users" },
];
