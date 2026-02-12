import { Validators } from "@angular/forms";

const passwordPattern = /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$/;

export const emailValidators = [
  Validators.required,
  Validators.email,
  Validators.pattern(/\.com$/i),
];

export const passwordValidators = [
  Validators.required,
  Validators.minLength(8),
  Validators.pattern(passwordPattern),
];

export const optionalPasswordValidators = [
  Validators.minLength(8),
  Validators.pattern(passwordPattern),
];
