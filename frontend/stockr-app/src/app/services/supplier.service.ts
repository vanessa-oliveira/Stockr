import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Supplier, CreateSupplierRequest, UpdateSupplierRequest } from '../models/supplier';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private apiUrl: string = "http://localhost:5138/api";

  constructor(private http: HttpClient) {}

  public getAllSuppliers(): Observable<Array<Supplier>> {
    return this.http.get<Array<Supplier>>(this.apiUrl + "/supplier");
  }

  public getSupplierById(id: string): Observable<Supplier> {
    return this.http.get<Supplier>(this.apiUrl + "/supplier/" + id);
  }

  public addSupplier(supplier: CreateSupplierRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl + "/supplier", supplier);
  }

  public updateSupplier(supplier: UpdateSupplierRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl + "/supplier/" + supplier.id, supplier);
  }

  public deleteSupplier(supplierId: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + "/supplier/" + supplierId);
  }
}