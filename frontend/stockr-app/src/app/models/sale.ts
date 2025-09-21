export enum SaleStatus {
  Draft = 0,
  Pending = 1,
  Confirmed = 2,
  PendingPayment = 3,
  Paid = 4,
  Completed = 9,
  Cancelled = 10
}

export interface SaleItem {
  id?: string;
  productId: string;
  productName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Sale {
  id: string;
  customerId: string;
  customerName?: string;
  salespersonId: string;
  salespersonName?: string;
  saleStatus: SaleStatus;
  totalAmount: number;
  saleDate: string;
  saleItems: SaleItem[];
}