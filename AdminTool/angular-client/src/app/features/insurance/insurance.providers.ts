import { Provider } from "@angular/core";
import { INSURANCE_REPOSITORY } from "./domain/insurance.repository";
import { InsuranceHttpRepository } from "./infrastructure/insurance-http.repository";

export const INSURANCE_PROVIDERS: Provider[] = [
  { provide: INSURANCE_REPOSITORY, useExisting: InsuranceHttpRepository },
];
