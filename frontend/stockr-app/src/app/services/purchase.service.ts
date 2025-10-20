import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Purchase } from '../models/purchase';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/pagination.model';

export interface CreatePurchaseItemRequest {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreatePurchaseRequest {
  supplierId: string;
  purchaseDate: string;
  notes?: string;
  invoiceNumber: string;
  purchaseItems: CreatePurchaseItemRequest[];
}

export interface UpdatePurchaseRequest {
  id: string;
  supplierId: string;
  purchaseDate: string;
  notes?: string;
  invoiceNumber: string;
  purchaseItems: CreatePurchaseItemRequest[];
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseService {
  private apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {}

  public getAllPurchases(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Purchase>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Purchase>>(this.apiUrl + "/purchase", { params });
  }

  public getPurchaseById(id: string): Observable<Purchase> {
    return this.http.get<Purchase>(this.apiUrl + "/purchase/" + id);
  }

  public getPurchasesBySupplier(supplierId: string): Observable<Array<Purchase>> {
    return this.http.get<Array<Purchase>>(this.apiUrl + "/purchase/supplier/" + supplierId);
  }

  public getPurchasesByPeriod(startDate: string, endDate: string): Observable<Array<Purchase>> {
    return this.http.get<Array<Purchase>>(
      `${this.apiUrl}/purchase/period?startDate=${startDate}&endDate=${endDate}`
    );
  }

  public getPurchasesByInvoiceNumber(invoiceNumber: string): Observable<Array<Purchase>> {
    return this.http.get<Array<Purchase>>(this.apiUrl + "/purchase/invoice/" + invoiceNumber);
  }

  public addPurchase(purchase: CreatePurchaseRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl + "/purchase", purchase);
  }

  public updatePurchase(purchase: UpdatePurchaseRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl + "/purchase/" + purchase.id, purchase);
  }

  public deletePurchase(purchaseId: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + "/purchase/" + purchaseId);
  }
}
