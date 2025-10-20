export interface LoginRequest {
  email: string;
  password: string;
}

export interface TenantSignupRequest {
  tenantName: string;
  planType: PlanType;
  adminName: string;
  adminEmail: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  tokenExpiration?: Date;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  roleName?: string;
  tenantId?: string;
  isBlocked?: boolean;
  active?: boolean;
}

export interface Tenant {
  id: string;
  name: string;
  planType: PlanType;
}

export enum UserRole {
  SystemAdmin = 0,
  TenantAdmin = 1,
  Manager = 10,
  Seller = 20,
  StockController = 30,
  Cashier = 40,
  Viewer = 50,
  User = 99
}

export enum PlanType {
  Basic,
  Standard,
  Enterprise
}
