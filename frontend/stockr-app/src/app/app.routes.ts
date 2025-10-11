import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { tenantAdminGuard } from './guards/tenant-admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'signup',
    loadComponent: () => import('./auth/signup/signup.component').then(m => m.SignupComponent)
  },
  {
    path: '',
    loadComponent: () => import('./main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./categories/category-list/category-list.component').then(m => m.CategoryListComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./products/product-list/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'suppliers',
        loadComponent: () => import('./suppliers/supplier-list/supplier-list.component').then(m => m.SupplierListComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./customers/customer-list/customer-list.component').then(m => m.CustomerListComponent)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./inventory/inventory-list/inventory-list.component').then(m => m.InventoryListComponent)
      },
      {
        path: 'purchases',
        loadComponent: () => import('./purchases/purchase-list/purchase-list.component').then(m => m.PurchaseListComponent)
      },
      {
        path: 'sales',
        loadComponent: () => import('./sales/sale-list/sale-list.component').then(m => m.SaleListComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./users/user-list/user-list.component').then(m => m.UserListComponent),
        canActivate: [tenantAdminGuard]
      },
      {
        path: 'account-settings/:id',
        loadComponent: () => import('./account-settings/account-settings.component').then(m => m.AccountSettingsComponent)
      },
    ]
  },
];
