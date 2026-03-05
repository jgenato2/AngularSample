import { Inject, Injectable } from "@angular/core";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class ListUsersQuery {
  constructor(@Inject(USER_REPOSITORY) private readonly userRepository: UserRepository) {}

  execute(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string) {
    return this.userRepository.list(sort, query);
  }
}
