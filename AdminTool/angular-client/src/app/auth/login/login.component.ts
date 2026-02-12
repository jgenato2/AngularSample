import { Component, inject } from "@angular/core";
import { FormBuilder, ReactiveFormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { CommonModule } from "@angular/common";
import { AuthService } from "../../core/auth.service";
import { emailValidators, passwordValidators } from "../../core/validators";

@Component({
  selector: "app-login",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.scss",
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  private alertTimeout: number | null = null;

  form = this.fb.group({
    email: ["", emailValidators],
    password: ["", passwordValidators],
  });

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit() {
    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    const { email, password } = this.form.getRawValue();

    this.auth.login(email!, password!).subscribe({
      next: () => {
        this.loading = false;
        this.clearAlert();
        this.router.navigateByUrl("/users");
      },
      error: () => {
        this.loading = false;
        this.setAlert("Invalid credentials.");
      },
    });
  }

  private setAlert(message: string, type: "danger" | "success" = "danger") {
    this.alertMessage = message;
    this.alertType = type;
    if (this.alertTimeout) {
      window.clearTimeout(this.alertTimeout);
    }
    this.alertTimeout = window.setTimeout(() => {
      this.alertMessage = null;
    }, 3000);
  }

  private clearAlert() {
    this.alertMessage = null;
    if (this.alertTimeout) {
      window.clearTimeout(this.alertTimeout);
      this.alertTimeout = null;
    }
  }
}
