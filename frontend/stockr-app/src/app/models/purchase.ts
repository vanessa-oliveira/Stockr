export interface Purchase {
  id: string;
  supplierId: string;
  supplierName: string;
  totalAmount: number;
  purchaseDate: string;
  notes?: string;
  invoiceNumber: string;
  purchaseItems: PurchaseItem[];
}

export interface PurchaseItem {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}