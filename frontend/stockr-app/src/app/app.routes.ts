import { Routes } from '@angular/router';
import {CategoryListComponent} from './categories/category-list/category-list.component';
import {MainLayoutComponent} from './main-layout/main-layout.component';

export const routes: Routes = [
  { path: '', component: MainLayoutComponent,
  children: [
    { path: 'categories', component: CategoryListComponent }
  ]
  },
];
