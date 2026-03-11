import { Injectable, inject } from "@angular/core";
import { UpdateUserPayload } from "../../domain/user.models";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class UpdateUserCommand {
  private readonly userRepository = inject<UserRepository>(USER_REPOSITORY);


  constructor() {}

  execute(id: string, payload: UpdateUserPayload) {
    return this.userRepository.update(id, payload);
  }
}
