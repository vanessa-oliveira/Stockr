import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Product, CreateProductRequest, UpdateProductRequest } from '../../models/product';
import { Category } from '../../models/category';
import { Supplier } from '../../models/supplier';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { SupplierService } from '../../services/supplier.service';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    SelectModule,
    TextareaModule,
    ReactiveFormsModule,
    ToastModule
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Input() product: Product | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() productChanged = new EventEmitter<void>();

  public productForm!: FormGroup;
  public isLoading = false;
  public allCategories: Category[] = [];
  public allSuppliers: Supplier[] = [];
  public availableCategories: Category[] = [];
  public availableSuppliers: Supplier[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private supplierService: SupplierService
  ) {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      sku: [''],
      description: ['', Validators.required],
      categoryId: ['', Validators.required],
      supplierId: [''],
      costPrice: [0, [Validators.required, Validators.min(0)]],
      salePrice: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['visible'] && this.visible) {
      this.loadCategories();
      this.loadSuppliers();
    }

    if (changes['product']) {
      if (this.product) {

        this.filterAvailableOptions();

        this.updateFormWithProduct();

      } else {
        this.productForm.reset({
          name: '',
          sku: '',
          description: '',
          categoryId: '',
          supplierId: '',
          costPrice: 0,
          salePrice: 0
        });
      }
    }
  }

  loadCategories() {
    this.categoryService.getAllCategories(1, 20).subscribe({
      next: (result) => {
        this.allCategories = Array.isArray(result.items) ? result.items : [];
        this.filterAvailableOptions();
        if (this.product) {
          this.updateFormWithProduct();
        }
      },
      error: (error) => {
        console.error('Erro ao carregar categorias:', error);
        this.allCategories = [];
        this.filterAvailableOptions();
      }
    });
  }

  loadSuppliers() {
    this.supplierService.getAllSuppliers(1, 20).subscribe({
      next: (result) => {
        this.allSuppliers = Array.isArray(result.items) ? result.items : [];
        this.filterAvailableOptions();
        if (this.product) {
          this.updateFormWithProduct();
        }
      },
      error: (error) => {
        console.error('Erro ao carregar fornecedores:', error);
        this.allSuppliers = [];
        this.filterAvailableOptions();
      }
    });
  }

  updateFormWithProduct() {
    if (!this.product) return;

    this.productForm.patchValue({
      name: this.product.name,
      sku: this.product.sku,
      description: this.product.description,
      categoryId: this.product?.category?.id,
      supplierId: this.product?.supplier?.id || '',
      costPrice: this.product.costPrice,
      salePrice: this.product.salePrice
    });
  }

  filterAvailableOptions() {
    if (!Array.isArray(this.allCategories)) {
      this.allCategories = [];
    }

    if (!Array.isArray(this.allSuppliers)) {
      this.allSuppliers = [];
    }

    this.availableCategories = [...this.allCategories];

    if (this.product && this.product?.category?.id) {
      const currentCategory = this.allCategories.find(c => c.id === this.product!.category!.id);
      if (currentCategory && !this.availableCategories.find(c => c.id === currentCategory.id)) {
        this.availableCategories = [currentCategory, ...this.availableCategories];
      }
    }

    this.availableSuppliers = [...this.allSuppliers];

    if (this.product && this.product?.supplier?.id) {
      const currentSupplier = this.allSuppliers.find(s => s.id === this.product!.supplier!.id);
      if (currentSupplier && !this.availableSuppliers.find(s => s.id === currentSupplier.id)) {
        this.availableSuppliers = [currentSupplier, ...this.availableSuppliers];
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
    if (this.productForm.invalid) return;

    this.isLoading = true;
    const formValue = this.productForm.value;

    if (this.product) {
      const updateRequest: UpdateProductRequest = {
        id: this.product.id!,
        name: formValue.name,
        sku: formValue.sku,
        description: formValue.description,
        categoryId: formValue.categoryId,
        supplierId: formValue.supplierId || undefined,
        costPrice: formValue.costPrice,
        salePrice: formValue.salePrice
      };

      this.productService.updateProduct(updateRequest).subscribe({
        next: () => {
          this.productChanged.emit();
          this.closeDialog();
          this.productForm.reset();
        },
        error: (error) => {
          console.error('Erro ao atualizar produto:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      const createRequest: CreateProductRequest = {
        name: formValue.name,
        sku: formValue.sku,
        description: formValue.description,
        categoryId: formValue.categoryId,
        supplierId: formValue.supplierId || undefined,
        costPrice: formValue.costPrice,
        salePrice: formValue.salePrice
      };

      this.productService.addProduct(createRequest).subscribe({
        next: () => {
          this.productChanged.emit();
          this.closeDialog();
          this.productForm.reset();
        },
        error: (error) => {
          console.error('Erro ao criar produto:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    }
  }
}
