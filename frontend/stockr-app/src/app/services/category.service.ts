import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Category} from '../models/category';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {
  }

  public getAllCategories(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Category>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Category>>(this.apiUrl + "/category", { params });
  }

  public addCategory(category: Category): Observable<void> {
    return this.http.post<void>(this.apiUrl + "/category", category);
  }

  public updateCategory(category: Category): Observable<void> {
    return this.http.put<void>(this.apiUrl + "/category/" + category.id, category);
  }

  public deleteCategory(categoryId: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + "/category/" + categoryId);
  }
}
