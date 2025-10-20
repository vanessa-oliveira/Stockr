import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Supplier, CreateSupplierRequest, UpdateSupplierRequest } from '../models/supplier';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {}

  public getAllSuppliers(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Supplier>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Supplier>>(this.apiUrl + "/supplier", { params });
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
