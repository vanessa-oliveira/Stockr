import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, CreateProductRequest, UpdateProductRequest } from '../models/product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl: string = "http://localhost:5138/api";

  constructor(private http: HttpClient) {}

  public getAllProducts(): Observable<Array<Product>> {
    return this.http.get<Array<Product>>(this.apiUrl + "/product");
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