import { inject } from "@angular/core";
import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
} from "@angular/common/http";
import { AuthService } from "./auth.service";

export const tokenInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const auth = inject(AuthService);
  const token = auth.getToken();
  if (!token) {
    return next(req);
  }

  const withAuth = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });
  return next(withAuth);
};
