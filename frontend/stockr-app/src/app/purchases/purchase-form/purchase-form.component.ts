import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnInit, ChangeDetectorRef } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { Purchase, PurchaseItem } from '../../models/purchase';
import { PurchaseService, CreatePurchaseRequest, UpdatePurchaseRequest, CreatePurchaseItemRequest } from '../../services/purchase.service';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { SupplierService } from '../../services/supplier.service';
import { ProductService } from '../../services/product.service';
import { Supplier } from '../../models/supplier';
import { Product } from '../../models/product';
import { CommonModule } from '@angular/common';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';

@Component({
  selector: 'app-purchase-form',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    ReactiveFormsModule,
    SelectModule,
    InputNumberModule,
    ToastModule,
    TextareaModule,
    DatePickerModule
  ],
  providers: [MessageService],
  templateUrl: './purchase-form.component.html',
  styleUrl: './purchase-form.component.scss'
})
export class PurchaseFormComponent implements OnInit, OnChanges {
  @Input() visible: boolean = false;
  @Input() purchase: Purchase | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() purchaseChanged = new EventEmitter<void>();

  public purchaseForm!: FormGroup;
  public isLoading = false;
  public suppliers: Supplier[] = [];
  public products: Product[] = [];

  constructor(
    private fb: FormBuilder,
    private purchaseService: PurchaseService,
    private supplierService: SupplierService,
    private productService: ProductService,
    private cdr: ChangeDetectorRef,
    private messageService: MessageService
  ) {
    this.purchaseForm = this.fb.group({
      supplierId: ['', Validators.required],
      purchaseDate: [new Date(), Validators.required],
      invoiceNumber: ['', Validators.required],
      notes: [''],
      purchaseItems: this.fb.array([], Validators.required)
    });
  }

  ngOnInit() {
    this.loadSuppliers();
    this.loadProducts();
  }

  get purchaseItems(): FormArray {
    return this.purchaseForm.get('purchaseItems') as FormArray;
  }

  get totalAmount(): number {
    return this.purchaseItems.controls.reduce((total, item) => {
      const quantity = item.get('quantity')?.value || 0;
      const unitPrice = item.get('unitPrice')?.value || 0;
      return total + (quantity * unitPrice);
    }, 0);
  }

  loadSuppliers() {
    this.supplierService.getAllSuppliers().subscribe({
      next: (suppliers) => this.suppliers = suppliers,
      error: (error) => console.error('Erro ao carregar fornecedores:', error)
    });
  }

  loadProducts() {
    this.productService.getAllProducts().subscribe({
      next: (products) => this.products = products,
      error: (error) => console.error('Erro ao carregar produtos:', error)
    });
  }

  createPurchaseItem(item?: PurchaseItem): FormGroup {
    return this.fb.group({
      productId: [item?.productId || '', Validators.required],
      quantity: [item?.quantity || 1, [Validators.required, Validators.min(1)]],
      unitPrice: [item?.unitPrice || 0, [Validators.required, Validators.min(0)]]
    });
  }

  addPurchaseItem() {
    this.purchaseItems.push(this.createPurchaseItem());
    this.cdr.detectChanges();
  }

  removePurchaseItem(index: number) {
    this.purchaseItems.removeAt(index);
    this.cdr.detectChanges();
  }

  onProductChange(index: number) {
    const item = this.purchaseItems.at(index);
    const productId = item.get('productId')?.value;

    if (productId) {
      const product = this.products.find(p => p.id === productId);
      if (product) {
        item.patchValue({ unitPrice: product.costPrice });
        this.cdr.detectChanges();
      }
    }
  }

  onValueChange() {
    this.cdr.detectChanges();
  }

  getSubtotal(index: number): number {
    const item = this.purchaseItems.at(index);
    const quantity = item.get('quantity')?.value || 0;
    const unitPrice = item.get('unitPrice')?.value || 0;
    return quantity * unitPrice;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['purchase'] && this.purchase) {
      this.purchaseForm.patchValue({
        supplierId: this.purchase.supplierId,
        purchaseDate: new Date(this.purchase.purchaseDate),
        invoiceNumber: this.purchase.invoiceNumber,
        notes: this.purchase.notes || ''
      });

      this.purchaseItems.clear();
      this.purchase.purchaseItems.forEach(item => {
        this.purchaseItems.push(this.createPurchaseItem(item));
      });
    } else if (changes['purchase'] && !this.purchase) {
      this.purchaseForm.reset({
        purchaseDate: new Date(),
        notes: ''
      });
      this.purchaseItems.clear();
    }
  }

  closeDialog() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
    this.cdr.detectChanges();
  }

  onVisibilityChange(visible: boolean) {
    this.visible = visible;
    this.visibleChange.emit(visible);
    this.cdr.detectChanges();
  }

  public onSubmit() {
    if (this.purchaseForm.invalid) return;

    this.isLoading = true;
    const formValue = this.purchaseForm.value;

    const purchaseItems: CreatePurchaseItemRequest[] = formValue.purchaseItems.map((item: any) => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice
    }));

    if (this.purchase) {
      const updateRequest: UpdatePurchaseRequest = {
        id: this.purchase.id,
        supplierId: formValue.supplierId,
        purchaseDate: formValue.purchaseDate,
        invoiceNumber: formValue.invoiceNumber,
        notes: formValue.notes,
        purchaseItems: purchaseItems
      };

      this.purchaseService.updatePurchase(updateRequest).subscribe({
        next: () => {
          this.purchaseChanged.emit();
          this.closeDialog();
          this.purchaseForm.reset();
          this.purchaseItems.clear();
          this.isLoading = false;
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Erro ao atualizar compra', detail: `${error.error?.message || error.error?.details || 'Erro desconhecido'}`, life: 3000 });
          this.isLoading = false;
        }
      });
    } else {
      const createRequest: CreatePurchaseRequest = {
        supplierId: formValue.supplierId,
        purchaseDate: formValue.purchaseDate,
        invoiceNumber: formValue.invoiceNumber,
        notes: formValue.notes,
        purchaseItems: purchaseItems
      };

      this.purchaseService.addPurchase(createRequest).subscribe({
        next: () => {
          this.purchaseChanged.emit();
          this.closeDialog();
          this.purchaseForm.reset();
          this.purchaseItems.clear();
          this.isLoading = false;
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Erro ao registrar compra', detail: `${error.error?.message || error.error?.details || 'Erro desconhecido'}`, life: 3000 });
          this.isLoading = false;
        }
      });
    }
  }
}