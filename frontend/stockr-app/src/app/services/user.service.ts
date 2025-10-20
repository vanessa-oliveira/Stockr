import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserRole } from '../models/auth.model';
import { PagedResult } from '../models/pagination.model';
import {ChangeUserPasswordRequest, CreateUserRequest, UpdatePersonalInfoRequest, User} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5138/api/user';

  constructor(private http: HttpClient) {}

  getAllUsers(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<User>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<User>>(`${this.apiUrl}`, { params });
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<any> {
    return this.http.post(this.apiUrl, request);
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

  changePassword(id: string, changeUserPasswordRequest: ChangeUserPasswordRequest): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/change-password`, changeUserPasswordRequest);
  }

  updatePersonalInfo(id: string, updatePersonalInfoRequest: UpdatePersonalInfoRequest){
    return this.http.patch(`${this.apiUrl}/${id}/update-personal-info`, updatePersonalInfoRequest);
  }
}
