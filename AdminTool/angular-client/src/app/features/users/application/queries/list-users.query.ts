import { Injectable, inject } from "@angular/core";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class ListUsersQuery {
  private readonly userRepository = inject<UserRepository>(USER_REPOSITORY);


  constructor() {}

  execute(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string) {
    return this.userRepository.list(sort, query);
  }
}
