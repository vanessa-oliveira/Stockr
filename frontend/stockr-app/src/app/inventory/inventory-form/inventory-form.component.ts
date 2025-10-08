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

    if (changes['inventory'] && this.inventory) {
      this.inventoryForm.patchValue({
        productId: this.inventory.productId,
        minStock: this.inventory.minStock,
        currentStock: this.inventory.currentStock
      });
      this.inventoryForm.get('productId')?.disable();
      this.inventoryForm.get('currentStock')?.disable();
    } else if (changes['inventory'] && !this.inventory) {
      this.inventoryForm.reset({
        minStock: 0,
        currentStock: 0
      });
      this.inventoryForm.get('productId')?.enable();
      this.inventoryForm.get('currentStock')?.enable();
    }
  }

  loadProducts() {
    this.productService.getAllProducts().subscribe({
      next: (products) => {
        this.products = products;
        this.filterAvailableProducts();
      },
      error: (error) => console.error('Erro ao carregar produtos:', error)
    });
  }

  loadInventory() {
    this.inventoryService.getAllInventory().subscribe({
      next: (inventory) => {
        this.allInventory = inventory;
        this.filterAvailableProducts();
      },
      error: (error) => console.error('Erro ao carregar estoque:', error)
    });
  }

  filterAvailableProducts() {
    if (this.products.length === 0 || this.allInventory.length === 0) {
      this.availableProducts = this.products;
      return;
    }

    const productsWithInventory = this.allInventory.map(inv => inv.productId);

    this.availableProducts = this.products.filter(
      product => !productsWithInventory.includes(product.id!)
    );
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
