import { ApplicationConfig, provideBrowserGlobalErrorListeners } from "@angular/core";
import { PreloadAllModules, provideRouter, withPreloading } from "@angular/router";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { routes } from "./app.routes";
import { tokenInterceptor } from "./core/token.interceptor";
import { CLAIMS_PROVIDERS } from "./features/claims/claims.providers";
import { INSURANCE_PROVIDERS } from "./features/insurance/insurance.providers";
import { USERS_PROVIDERS } from "./features/users/users.providers";

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withPreloading(PreloadAllModules)),
    provideHttpClient(withInterceptors([tokenInterceptor])),
    ...CLAIMS_PROVIDERS,
    ...INSURANCE_PROVIDERS,
    ...USERS_PROVIDERS,
  ]
};
