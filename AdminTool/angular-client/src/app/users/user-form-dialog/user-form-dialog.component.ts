import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { UserItem } from "../../features/users/domain/user.models";
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
  ],
  templateUrl: "./user-form-dialog.component.html",
  styleUrl: "./user-form-dialog.component.scss",
})
export class UserFormDialogComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) data!: UserDialogData;
  @Input() modalId = "user-form-modal";
  @Output() submitted = new EventEmitter<{
    name: string;
    email: string;
    role: "admin" | "user";
    password: string;
  }>();
  @Output() canceled = new EventEmitter<void>();

  form = this.fb.group({
    name: ["", Validators.required],
    email: ["", emailValidators],
    role: ["user", Validators.required],
    password: ["", optionalPasswordValidators],
  });

  ngOnChanges(changes: SimpleChanges) {
    if (changes["data"] && this.data) {
      this.resetForm();
    }
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.submitted.emit({
      name: value.name ?? "",
      email: value.email ?? "",
      role: (value.role as "admin" | "user") ?? "user",
      password: value.password ?? "",
    });
  }

  cancel() {
    this.canceled.emit();
  }

  private resetForm() {
    this.form.reset({
      name: "",
      email: "",
      role: "user",
      password: "",
    });

    if (this.data.user) {
      this.form.patchValue({
        name: this.data.user.name,
        email: this.data.user.email,
        role: this.data.user.role,
      });
    }

    if (this.data.isAdmin) {
      this.form.get("role")?.enable({ emitEvent: false });
    } else {
      this.form.get("role")?.disable({ emitEvent: false });
    }

    if (this.data.mode === "create") {
      this.form.get("password")?.setValidators(passwordValidators);
    } else {
      this.form.get("password")?.setValidators(optionalPasswordValidators);
    }

    this.form.updateValueAndValidity();
  }
}
