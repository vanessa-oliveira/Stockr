import { Routes } from '@angular/router';
import {CategoryListComponent} from './categories/category-list/category-list.component';
import {MainLayoutComponent} from './main-layout/main-layout.component';
import {ProductListComponent} from './products/product-list/product-list.component';
import {SupplierListComponent} from './suppliers/supplier-list/supplier-list.component';
import {CustomerListComponent} from './customers/customer-list/customer-list.component';
import {SaleListComponent} from './sales/sale-list/sale-list.component';
import {InventoryListComponent} from './inventory/inventory-list/inventory-list.component';
import {PurchaseListComponent} from './purchases/purchase-list/purchase-list.component';
import {LoginComponent} from './auth/login/login.component';
import {SignupComponent} from './auth/signup/signup.component';
import {authGuard} from './guards/auth.guard';
import {tenantAdminGuard} from './guards/tenant-admin.guard';
import {UserListComponent} from './users/user-list/user-list.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'products', pathMatch: 'full' },
      { path: 'categories', component: CategoryListComponent },
      { path: 'products', component: ProductListComponent },
      { path: 'suppliers', component: SupplierListComponent },
      { path: 'customers', component: CustomerListComponent },
      { path: 'inventory', component: InventoryListComponent },
      { path: 'purchases', component: PurchaseListComponent },
      { path: 'sales', component: SaleListComponent },
      { path: 'users', component: UserListComponent, canActivate: [tenantAdminGuard] },
    ]
  },
];
