import { Injectable, inject } from "@angular/core";
import { CreateUserPayload } from "../../domain/user.models";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class CreateUserCommand {
  private readonly userRepository = inject<UserRepository>(USER_REPOSITORY);


  constructor() {}

  execute(payload: CreateUserPayload) {
    return this.userRepository.create(payload);
  }
}
