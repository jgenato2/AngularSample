import { Component, inject } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { MatButtonModule } from "@angular/material/button";
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatSelectModule } from "@angular/material/select";
import { UserItem } from "../users.service";
import {
  emailValidators,
  optionalPasswordValidators,
  passwordValidators,
} from "../../core/validators";

export interface UserDialogData {
  mode: "create" | "edit";
  user?: UserItem;
  isAdmin: boolean;
}

@Component({
  selector: "app-user-form-dialog",
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: "./user-form-dialog.component.html",
  styleUrl: "./user-form-dialog.component.scss",
})
export class UserFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  readonly data = inject<UserDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject(MatDialogRef<UserFormDialogComponent>);

  form = this.fb.group({
    name: ["", Validators.required],
    email: ["", emailValidators],
    role: [{ value: "user", disabled: !this.data.isAdmin }, Validators.required],
    password: ["", optionalPasswordValidators],
  });

  constructor() {
    const data = this.data;

    if (data.user) {
      this.form.patchValue({
        name: data.user.name,
        email: data.user.email,
        role: data.user.role,
      });
    }

    if (!data.isAdmin) {
      this.form.get("role")?.disable({ emitEvent: false });
    }

    if (data.mode === "create") {
      this.form.get("password")?.setValidators(passwordValidators);
      this.form.updateValueAndValidity();
    }
  }

  submit() {
    if (this.form.invalid) {
      return;
    }
    const value = this.form.getRawValue();
    this.dialogRef.close(value);
  }
}
