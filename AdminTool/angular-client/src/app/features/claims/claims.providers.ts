import { Provider } from "@angular/core";
import { CLAIM_REPOSITORY } from "./domain/claim.repository";
import { ClaimHttpRepository } from "./infrastructure/claim-http.repository";

export const CLAIMS_PROVIDERS: Provider[] = [
  { provide: CLAIM_REPOSITORY, useExisting: ClaimHttpRepository },
];
