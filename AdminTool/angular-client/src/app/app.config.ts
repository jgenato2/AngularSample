import { ApplicationConfig } from "@angular/core";
import { CORE_PROVIDERS } from "./core/core.providers";
import { CLAIMS_PROVIDERS } from "./features/claims/claims.providers";
import { INSURANCE_PROVIDERS } from "./features/insurance/insurance.providers";
import { USERS_PROVIDERS } from "./features/users/users.providers";

export const appConfig: ApplicationConfig = {
  providers: [
    // Core: router, HTTP client, global error listeners
    ...CORE_PROVIDERS,

    // Feature: DI token bindings (repository abstractions → implementations).
    // These stay at root because the facades that consume them are providedIn:'root'.
    ...CLAIMS_PROVIDERS,
    ...INSURANCE_PROVIDERS,
    ...USERS_PROVIDERS,
  ],
};
