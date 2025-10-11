import {UserRole} from './auth.model';
export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  roleName: string;
  isBlocked: boolean;
  active: boolean;
  tenantId?: string;
  createdAt?: Date;
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UpdatePersonalInfoRequest {
  id: string;
  name: string;
  email: string;
}

export interface ChangeUserPasswordRequest {
  id: string;
  currentPassword: string;
  newPassword: string;
}
