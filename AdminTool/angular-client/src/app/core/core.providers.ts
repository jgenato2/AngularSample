import { EnvironmentProviders } from "@angular/core";
import { PreloadAllModules, provideRouter, withPreloading } from "@angular/router";
import { provideHttpClient, withInterceptors } from "@angular/common/http";
import { provideBrowserGlobalErrorListeners } from "@angular/core";
import { routes } from "../app.routes";
import { tokenInterceptor } from "./token.interceptor";

/**
 * Core infrastructure providers – router, HTTP client, error listeners.
 * Registered once at the root injector level in app.config.ts.
 */
export const CORE_PROVIDERS: EnvironmentProviders[] = [
  provideBrowserGlobalErrorListeners(),
  provideRouter(routes, withPreloading(PreloadAllModules)),
  provideHttpClient(withInterceptors([tokenInterceptor])),
];
