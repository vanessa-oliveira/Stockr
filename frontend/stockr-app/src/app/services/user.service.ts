import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserRole } from '../models/auth.model';
import { PagedResult } from '../models/pagination.model';

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UserListItem {
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

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5138/api/user';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<UserListItem[]> {
    return this.http.get<UserListItem[]>(this.apiUrl);
  }

  getUsersPaged(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<UserListItem>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<UserListItem>>(`${this.apiUrl}`, { params });
  }

  getUserById(id: string): Observable<UserListItem> {
    return this.http.get<UserListItem>(`${this.apiUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<any> {
    return this.http.post(this.apiUrl, request);
  }

  updateUser(id: string, request: Partial<CreateUserRequest>): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, request);
  }

  deleteUser(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  blockUser(id: string, blockedUntil?: Date): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/block`, { blockedUntil });
  }

  unblockUser(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/unblock`, {});
  }

  changePassword(id: string, newPassword: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/change-password`, { newPassword });
  }
}
