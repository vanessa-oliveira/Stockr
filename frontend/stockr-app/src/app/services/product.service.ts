import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, CreateProductRequest, UpdateProductRequest } from '../models/product';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {}

  public getAllProducts(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Product>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Product>>(this.apiUrl + "/product/paged", { params });
  }

  public getProductById(id: string): Observable<Product> {
    return this.http.get<Product>(this.apiUrl + "/product/" + id);
  }

  public addProduct(product: CreateProductRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl + "/product", product);
  }

  public updateProduct(product: UpdateProductRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl + "/product/" + product.id, product);
  }

  public deleteProduct(productId: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + "/product/" + productId);
  }
}
