import { Routes } from '@angular/router';
import {CategoryListComponent} from './categories/category-list/category-list.component';
import {MainLayoutComponent} from './main-layout/main-layout.component';
import {ProductListComponent} from './products/product-list/product-list.component';
import {SupplierListComponent} from './suppliers/supplier-list/supplier-list.component';

export const routes: Routes = [
  { path: '', component: MainLayoutComponent,
  children: [
    { path: 'categories', component: CategoryListComponent },
    { path: 'products', component: ProductListComponent },
    { path: 'suppliers', component: SupplierListComponent },
  ]
  },
];
