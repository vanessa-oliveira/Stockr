import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Sale } from '../models/sale';
import { environment } from '../../environments/environment';

export interface CreateSaleItemRequest {
  productId: string;
  quantity: number;
  unitPrice?: number;
}

export interface CreateSaleRequest {
  customerId?: string;
  salespersonId?: string;
  saleStatus?: string;
  saleDate?: string;
  saleItems: CreateSaleItemRequest[];
}

export interface UpdateSaleRequest {
  id: string;
  customerId?: string;
  salespersonId?: string;
  saleStatus?: string;
  saleDate?: string;
  saleItems: CreateSaleItemRequest[];
}

export interface ChangeSaleStatusRequest {
  saleStatus: string;
}

@Injectable({
  providedIn: 'root'
})
export class SaleService {
  private apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {}

  public getAllSales(): Observable<Array<Sale>> {
    return this.http.get<Array<Sale>>(this.apiUrl + "/sale");
  }

  public getSaleById(id: string): Observable<Sale> {
    return this.http.get<Sale>(this.apiUrl + "/sale/" + id);
  }

  public getSalesByCustomer(customerId: string): Observable<Array<Sale>> {
    return this.http.get<Array<Sale>>(this.apiUrl + "/sale/customer/" + customerId);
  }

  public getSalesBySalesperson(userId: string): Observable<Array<Sale>> {
    return this.http.get<Array<Sale>>(this.apiUrl + "/sale/salesperson/" + userId);
  }

  public getSalesByPeriod(startDate: string, endDate: string): Observable<Array<Sale>> {
    return this.http.get<Array<Sale>>(
      `${this.apiUrl}/sale/period?startDate=${startDate}&endDate=${endDate}`
    );
  }

  public addSale(sale: CreateSaleRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl + "/sale", sale);
  }

  public updateSale(sale: UpdateSaleRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl + "/sale/" + sale.id, sale);
  }

  public changeSaleStatus(saleId: string, request: ChangeSaleStatusRequest): Observable<void> {
    return this.http.patch<void>(this.apiUrl + "/sale/" + saleId + "/status", request);
  }

  public deleteSale(saleId: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + "/sale/" + saleId);
  }
}
