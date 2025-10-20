import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Inventory } from '../models/inventory';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/pagination.model';

export interface CreateInventoryRequest {
  productId: string;
  minStock: number;
  currentStock: number;
}

export interface UpdateInventoryRequest {
  id: string;
  productId: string;
  minStock: number;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl = `${environment.apiUrl}/inventory`;

  constructor(private http: HttpClient) { }

  getAllInventory(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Inventory>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Inventory>>(`${this.apiUrl}`, { params });
  }

  getInventoryById(id: string): Observable<Inventory> {
    return this.http.get<Inventory>(`${this.apiUrl}/${id}`);
  }

  getInventoryByProductId(productId: string): Observable<Inventory> {
    return this.http.get<Inventory>(`${this.apiUrl}/product/${productId}`);
  }

  addInventory(inventory: CreateInventoryRequest): Observable<any> {
    return this.http.post(this.apiUrl, inventory);
  }

  updateInventory(inventory: UpdateInventoryRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${inventory.id}`, inventory);
  }

  deleteInventory(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
