import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { CurrencyPipe } from '@angular/common';
import { Product } from '../../models/product';
import { ProductService } from '../../services/product.service';
import { ProductFormComponent } from '../product-form/product-form.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
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

  constructor(
    private productService: ProductService,
    private confirmationService: ConfirmationService
  ) {
    this.loadProducts();
  }

  loadProducts() {
    this.productService.getAllProducts().subscribe({
      next: (result) => this.products = result,
      error: (error) => console.error('Erro ao carregar produtos:', error)
    });
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
