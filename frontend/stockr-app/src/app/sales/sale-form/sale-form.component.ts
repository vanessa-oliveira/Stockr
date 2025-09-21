import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnInit, ChangeDetectorRef } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { Sale, SaleItem, SaleStatus } from '../../models/sale';
import { SaleService, CreateSaleRequest, UpdateSaleRequest, CreateSaleItemRequest } from '../../services/sale.service';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { CustomerService } from '../../services/customer.service';
import { ProductService } from '../../services/product.service';
import { Customer } from '../../models/customer';
import { Product } from '../../models/product';
import { CommonModule } from '@angular/common';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DatePickerModule } from 'primeng/datepicker';

@Component({
  selector: 'app-sale-form',
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
    DatePickerModule
  ],
  providers: [MessageService],
  templateUrl: './sale-form.component.html',
  styleUrl: './sale-form.component.scss'
})
export class SaleFormComponent implements OnInit, OnChanges {
  @Input() visible: boolean = false;
  @Input() sale: Sale | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() saleChanged = new EventEmitter<void>();

  public saleForm!: FormGroup;
  public isLoading = false;
  public customers: Customer[] = [];
  public products: Product[] = [];
  public saleStatuses = [
    { label: 'Rascunho', value: 'Draft' },
    { label: 'Pendente', value: 'Pending' },
    { label: 'Confirmada', value: 'Confirmed' },
    { label: 'Aguardando Pagamento', value: 'PendingPayment' },
    { label: 'Pago', value: 'Paid' },
    { label: 'Finalizada', value: 'Completed' },
    { label: 'Cancelada', value: 'Cancelled' }
  ];

  constructor(
    private fb: FormBuilder,
    private saleService: SaleService,
    private customerService: CustomerService,
    private productService: ProductService,
    private cdr: ChangeDetectorRef,
    private messageService: MessageService
  ) {
    this.saleForm = this.fb.group({
      customerId: [''],
      saleStatus: ['Pending', Validators.required],
      saleDate: [new Date(), Validators.required],
      saleItems: this.fb.array([], Validators.required)
    });
  }

  ngOnInit() {
    this.loadCustomers();
    this.loadProducts();
  }

  get saleItems(): FormArray {
    return this.saleForm.get('saleItems') as FormArray;
  }

  get totalAmount(): number {
    return this.saleItems.controls.reduce((total, item) => {
      const quantity = item.get('quantity')?.value || 0;
      const unitPrice = item.get('unitPrice')?.value || 0;
      return total + (quantity * unitPrice);
    }, 0);
  }

  loadCustomers() {
    this.customerService.getAllCustomers().subscribe({
      next: (customers) => this.customers = customers,
      error: (error) => console.error('Erro ao carregar clientes:', error)
    });
  }

  loadProducts() {
    this.productService.getAllProducts().subscribe({
      next: (products) => this.products = products,
      error: (error) => console.error('Erro ao carregar produtos:', error)
    });
  }

  createSaleItem(item?: SaleItem): FormGroup {
    return this.fb.group({
      productId: [item?.productId || '', Validators.required],
      quantity: [item?.quantity || 1, [Validators.required, Validators.min(1)]],
      unitPrice: [item?.unitPrice || 0, [Validators.required, Validators.min(0)]]
    });
  }

  addSaleItem() {
    this.saleItems.push(this.createSaleItem());
    this.cdr.detectChanges();
  }

  removeSaleItem(index: number) {
    this.saleItems.removeAt(index);
    this.cdr.detectChanges();
  }

  onProductChange(index: number) {
    const item = this.saleItems.at(index);
    const productId = item.get('productId')?.value;

    if (productId) {
      const product = this.products.find(p => p.id === productId);
      if (product) {
        item.patchValue({ unitPrice: product.salePrice });
        this.cdr.detectChanges();
      }
    }
  }

  onValueChange() {
    this.cdr.detectChanges();
  }

  getSubtotal(index: number): number {
    const item = this.saleItems.at(index);
    const quantity = item.get('quantity')?.value || 0;
    const unitPrice = item.get('unitPrice')?.value || 0;
    return quantity * unitPrice;
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['sale'] && this.sale) {
      this.saleForm.patchValue({
        customerId: this.sale.customerId,
        saleStatus: SaleStatus[this.sale.saleStatus],
        saleDate: new Date(this.sale.saleDate)
      });

      this.saleItems.clear();
      this.sale.saleItems.forEach(item => {
        this.saleItems.push(this.createSaleItem(item));
      });
    } else if (changes['sale'] && !this.sale) {
      this.saleForm.reset({
        saleStatus: 'Pending',
        saleDate: new Date()
      });
      this.saleItems.clear();
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
    if (this.saleForm.invalid) return;

    this.isLoading = true;
    const formValue = this.saleForm.value;

    const saleItems: CreateSaleItemRequest[] = formValue.saleItems.map((item: any) => ({
      productId: item.productId,
      quantity: item.quantity,
      unitPrice: item.unitPrice
    }));

    if (this.sale) {
      const updateRequest: UpdateSaleRequest = {
        id: this.sale.id,
        customerId: formValue.customerId,
        saleStatus: formValue.saleStatus,
        saleDate: formValue.saleDate,
        saleItems: saleItems
      };

      this.saleService.updateSale(updateRequest).subscribe({
        next: () => {
          this.saleChanged.emit();
          this.closeDialog();
          this.saleForm.reset();
          this.saleItems.clear();
          this.isLoading = false;
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Erro ao atualizar venda', detail: `${error.error?.message || error.error?.details || 'Erro desconhecido'}`, life: 3000 });
          this.isLoading = false;
        }
      });
    } else {
      const createRequest: CreateSaleRequest = {
        customerId: formValue.customerId,
        saleStatus: formValue.saleStatus,
        saleDate: formValue.saleDate,
        saleItems: saleItems
      };

      this.saleService.addSale(createRequest).subscribe({
        next: () => {
          this.saleChanged.emit();
          this.closeDialog();
          this.saleForm.reset();
          this.saleItems.clear();
          this.isLoading = false;
        },
        error: (error) => {
          this.messageService.add({ severity: 'error', summary: 'Erro ao registrar venda', detail: `${error.error?.message + ' '+ error.error?.details || 'Erro desconhecido'}`, life: 3000 });
          this.isLoading = false;
        }
      });
    }
  }
}
