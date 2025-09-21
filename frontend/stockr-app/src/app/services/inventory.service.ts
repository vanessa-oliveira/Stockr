import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Inventory } from '../models/inventory';
import { environment } from '../../environments/environment';

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

  getAllInventory(): Observable<Inventory[]> {
    return this.http.get<Inventory[]>(this.apiUrl);
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