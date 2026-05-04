import { Routes } from "@angular/router";
import { authGuard } from "../../core/auth.guard";

export const USERS_ROUTES: Routes = [
  {
    path: "",
    canActivate: [authGuard],
    loadComponent: () => import("./user-list/user-list.component").then((m) => m.UserListComponent),
  },
  {
    path: ":id",
    canActivate: [authGuard],
    loadComponent: () => import("./user-detail/user-detail.component").then((m) => m.UserDetailComponent),
  },
];
