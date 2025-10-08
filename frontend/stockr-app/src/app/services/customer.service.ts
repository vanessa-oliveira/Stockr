import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Customer } from '../models/customer';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/pagination.model';

export interface CreateCustomerRequest {
  name: string;
  email: string;
  phone: string;
  cpf?: string;
  cnpj?: string;
}

export interface UpdateCustomerRequest {
  id: string;
  name: string;
  email: string;
  phone: string;
  cpf?: string;
  cnpj?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  private apiUrl: string = environment.apiUrl;

  constructor(private http: HttpClient) {}

  public getAllCustomers(): Observable<Array<Customer>> {
    return this.http.get<Array<Customer>>(this.apiUrl + "/customer");
  }

  public getCustomersPaged(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Customer>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Customer>>(this.apiUrl + "/customer", { params });
  }

  public getCustomerById(id: string): Observable<Customer> {
    return this.http.get<Customer>(this.apiUrl + "/customer/" + id);
  }

  public addCustomer(customer: CreateCustomerRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl + "/customer", customer);
  }

  public updateCustomer(customer: UpdateCustomerRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl + "/customer/" + customer.id, customer);
  }

  public deleteCustomer(customerId: string): Observable<void> {
    return this.http.delete<void>(this.apiUrl + "/customer/" + customerId);
  }
}
