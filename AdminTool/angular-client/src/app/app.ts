import { Component } from "@angular/core";
import { Router, RouterLink, RouterOutlet } from "@angular/router";
import { CommonModule } from "@angular/common";
import { AuthService } from "./core/auth.service";

@Component({
  selector: "app-root",
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: "./app.html",
  styleUrl: "./app.scss",
})
export class App {
  constructor(public readonly auth: AuthService, private readonly router: Router) {}

  logout() {
    this.auth.logout();
    this.router.navigateByUrl("/login");
  }
}
