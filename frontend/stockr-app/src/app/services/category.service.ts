import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Category} from '../models/category';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  public apiUrl: string = "http://localhost:5138/api";

  constructor(private http: HttpClient) {
  }

  public getAllCategories(): Observable<Array<Category>> {
    return this.http.get<Array<Category>>(this.apiUrl + "/category");
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
