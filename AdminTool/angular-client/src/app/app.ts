import { Component, HostListener, inject } from "@angular/core";
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
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  menuOpen = false;
  userMenuOpen = false;


  constructor() {}

  get isLoginPage() {
    return this.router.url.startsWith("/login");
  }

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
    if (this.menuOpen) {
      this.userMenuOpen = false;
    }
  }

  closeMenu() {
    this.menuOpen = false;
  }

  toggleUserMenu(event: MouseEvent) {
    event.stopPropagation();
    this.userMenuOpen = !this.userMenuOpen;
    if (this.userMenuOpen) {
      this.menuOpen = false;
    }
  }

  closeUserMenu() {
    this.userMenuOpen = false;
  }

  goToProfile() {
    const userId = this.auth.user()?.id;
    this.userMenuOpen = false;
    if (userId) {
      this.router.navigate(["/users", userId]);
      return;
    }

    this.router.navigateByUrl("/users");
  }

  logout() {
    this.auth.logout();
    this.menuOpen = false;
    this.userMenuOpen = false;
    this.router.navigateByUrl("/login");
  }

  @HostListener("document:click")
  onDocumentClick() {
    this.userMenuOpen = false;
  }
}
