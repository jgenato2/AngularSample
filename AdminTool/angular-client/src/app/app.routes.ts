import { Routes } from "@angular/router";
import { LoginComponent } from "./auth/login/login.component";
import { UserListComponent } from "./users/user-list/user-list.component";
import { UserDetailComponent } from "./users/user-detail/user-detail.component";
import { InsuranceListComponent } from "./insurance/insurance-list/insurance-list.component";
import { InsuranceDetailComponent } from "./insurance/insurance-detail/insurance-detail.component";
import { authGuard } from "./core/auth.guard";

export const routes: Routes = [
  { path: "", pathMatch: "full", redirectTo: "users" },
  { path: "login", component: LoginComponent },
  { path: "users", component: UserListComponent, canActivate: [authGuard] },
  { path: "users/:id", component: UserDetailComponent, canActivate: [authGuard] },
  { path: "insurance", component: InsuranceListComponent, canActivate: [authGuard] },
  { path: "insurance/:policyId", component: InsuranceDetailComponent, canActivate: [authGuard] },
  { path: "**", redirectTo: "users" },
];
