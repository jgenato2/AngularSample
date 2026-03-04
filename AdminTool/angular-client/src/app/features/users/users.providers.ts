import { Provider } from "@angular/core";
import { USER_REPOSITORY } from "./domain/user.repository";
import { UserHttpRepository } from "./infrastructure/user-http.repository";

export const USERS_PROVIDERS: Provider[] = [
  { provide: USER_REPOSITORY, useExisting: UserHttpRepository },
];
