export interface Supplier {
  id: string;
  name: string;
  phone: string;
}

export interface CreateSupplierRequest {
  name: string;
  phone: string;
}

export interface UpdateSupplierRequest {
  id: string;
  name: string;
  phone: string;
}