import { Injectable, inject } from "@angular/core";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class GetUserByIdQuery {
  private readonly userRepository = inject<UserRepository>(USER_REPOSITORY);


  constructor() {}

  execute(id: string) {
    return this.userRepository.getById(id);
  }
}
