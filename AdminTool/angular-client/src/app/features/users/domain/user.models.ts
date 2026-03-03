import { AuthUser } from "../../../core/auth.service";

export interface UserItem extends AuthUser {
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserPayload {
  name: string;
  email: string;
  role: "admin" | "user";
  password: string;
}

export interface UpdateUserPayload {
  name?: string;
  email?: string;
  role?: "admin" | "user";
  password?: string;
}

export interface UserAuditLogItem {
  id: string;
  action: string;
  field: string;
  oldValue?: string | null;
  newValue?: string | null;
  performedBy: string;
  occurredAtUtc: string;
}
