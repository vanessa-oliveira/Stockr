import { Component } from '@angular/core';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { CurrencyPipe } from '@angular/common';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { ProductFormComponent } from '../product-form/product-form.component';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    ProductFormComponent,
    CurrencyPipe
  ],
  providers: [ConfirmationService],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss'
})
export class ProductListComponent {
  public products: Array<Product> = [];
  public visible: boolean = false;
  public selectedProduct: Product | null = null;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  constructor(
    private productService: ProductService,
    private confirmationService: ConfirmationService
  ) {
    this.loadProducts();
  }

  loadProducts() {
    this.productService.getProductsPaged(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<Product>) => {
        this.products = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Erro ao carregar produtos:', error)
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadProducts();
    }
  }

  public openProductForm() {
    this.selectedProduct = null;
    this.visible = true;
  }

  public editProduct(product: Product) {
    this.selectedProduct = product;
    this.visible = true;
  }

  public deleteProduct(product: Product) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o produto "${product.name}"? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (product.id) {
          this.productService.deleteProduct(product.id).subscribe({
            next: () => {
              this.loadProducts();
            },
            error: (error) => {
              console.error('Erro ao excluir produto:', error);
            }
          });
        }
      }
    });
  }
}
