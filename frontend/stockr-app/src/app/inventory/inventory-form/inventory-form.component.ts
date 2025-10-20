import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnInit } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Inventory } from '../../models/inventory';
import { Product } from '../../models/product';
import { InventoryService, CreateInventoryRequest, UpdateInventoryRequest } from '../../services/inventory.service';
import { ProductService } from '../../services/product.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-inventory-form',
  standalone: true,
  imports: [
    DialogModule,
    ButtonModule,
    InputNumberModule,
    SelectModule,
    ReactiveFormsModule
  ],
  providers: [MessageService],
  templateUrl: './inventory-form.component.html',
  styleUrl: './inventory-form.component.scss'
})
export class InventoryFormComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Input() inventory: Inventory | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() inventoryChanged = new EventEmitter<void>();

  public inventoryForm!: FormGroup;
  public isLoading = false;
  public products: Product[] = [];
  public availableProducts: Product[] = [];
  private allInventory: Inventory[] = [];

  constructor(
    private fb: FormBuilder,
    private inventoryService: InventoryService,
    private productService: ProductService,
    private messageService: MessageService
  ) {
    this.inventoryForm = this.fb.group({
      productId: ['', Validators.required],
      minStock: [0, [Validators.required, Validators.min(1)]],
      currentStock: [0, [Validators.required, Validators.min(0)]]
    });
  }

  get currentStockQuantity(){
    return this.inventoryForm.get('currentStock')?.value;
  }

  get minStockQuantity(){
    return this.inventoryForm.get('minStock')?.value;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['visible'] && this.visible) {
      this.loadProducts();
      this.loadInventory();
    }

    if (changes['inventory']) {
      if (this.inventory) {
        this.filterAvailableProducts();

        this.inventoryForm.patchValue({
          productId: this.inventory.productId,
          minStock: this.inventory.minStock,
          currentStock: this.inventory.currentStock
        });
        this.inventoryForm.get('currentStock')?.disable();
      } else {
        this.inventoryForm.reset({
          minStock: 0,
          currentStock: 0
        });
        this.inventoryForm.get('currentStock')?.enable();

        this.filterAvailableProducts();
      }
    }
  }

  loadProducts() {
    this.productService.getAllProducts(1, 20).subscribe({
      next: (result) => {
        this.products = result.items;
        this.filterAvailableProducts();
      },
      error: (error) => console.error('Erro ao carregar produtos:', error)
    });
  }

  loadInventory() {
    this.inventoryService.getAllInventory(1, 20).subscribe({
      next: (result) => {
        this.allInventory = Array.isArray(result.items) ? result.items : [];
        this.filterAvailableProducts();
      },
      error: (error) => console.error('Erro ao carregar estoque:', error)
    });
  }

  filterAvailableProducts() {
    if (!Array.isArray(this.allInventory)) {
      this.allInventory = [];
    }

    if (this.products.length === 0) {
      this.availableProducts = [];
      return;
    }

    if (this.allInventory.length === 0) {
      this.availableProducts = this.products;
      return;
    }

    const productsWithInventory = this.allInventory.map(inv => inv.productId);

    this.availableProducts = this.products.filter(
      product => !productsWithInventory.includes(product.id!)
    );

    if (this.inventory && this.inventory.productId) {
      const currentProduct = this.products.find(p => p.id === this.inventory!.productId);
      if (currentProduct && !this.availableProducts.find(p => p.id === currentProduct.id)) {
        this.availableProducts = [currentProduct, ...this.availableProducts];
      }
    }
  }

  closeDialog() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  onVisibilityChange(visible: boolean) {
    this.visible = visible;
    this.visibleChange.emit(visible);
  }

  public onSubmit() {
    if (this.inventoryForm.invalid) return;

    this.isLoading = true;
    const formValue = this.inventoryForm.getRawValue();

    if (this.inventory) {
      const updateRequest: UpdateInventoryRequest = {
        id: this.inventory.id,
        productId: formValue.productId,
        minStock: formValue.minStock
      };

      this.inventoryService.updateInventory(updateRequest).subscribe({
        next: () => {
          this.inventoryChanged.emit();
          this.closeDialog();
          this.inventoryForm.reset();
        },
        error: (error) => {
          console.error('Erro ao atualizar estoque:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      if(formValue.currentStock < formValue.minStock){
        this.messageService.add({ severity: 'error', summary: 'Erro', detail: 'Estoque atual deve ser maior que o estoque mínimo', life: 3000 });
        return;
      }

      const createRequest: CreateInventoryRequest = {
        productId: formValue.productId,
        minStock: formValue.minStock,
        currentStock: formValue.currentStock
      };

      this.inventoryService.addInventory(createRequest).subscribe({
        next: () => {
          this.inventoryChanged.emit();
          this.closeDialog();
          this.inventoryForm.reset();
        },
        error: (error) => {
          console.error('Erro ao criar estoque:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    }
  }
}
