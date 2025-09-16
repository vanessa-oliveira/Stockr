import { Category } from './category';
import { Supplier } from './supplier';

export interface Product {
  id?: string;
  name: string;
  sku: string;
  description: string;
  categoryId: string;
  supplierId?: string;
  costPrice: number;
  salePrice: number;
  category?: Category;
  supplier?: Supplier;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  description: string;
  categoryId: string;
  supplierId?: string;
  costPrice: number;
  salePrice: number;
}

export interface UpdateProductRequest {
  id: string;
  name: string;
  sku: string;
  description: string;
  categoryId: string;
  supplierId?: string;
  costPrice: number;
  salePrice: number;
}