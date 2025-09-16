import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnInit } from '@angular/core';
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
    ReactiveFormsModule
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnChanges, OnInit {
  @Input() visible: boolean = false;
  @Input() product: Product | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() productChanged = new EventEmitter<void>();

  public productForm!: FormGroup;
  public isLoading = false;
  public categories: Category[] = [];
  public suppliers: Supplier[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private supplierService: SupplierService
  ) {
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      sku: ['', Validators.required],
      description: ['', Validators.required],
      categoryId: ['', Validators.required],
      supplierId: [''],
      costPrice: [0, [Validators.required, Validators.min(0)]],
      salePrice: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit() {
    this.loadCategories();
    this.loadSuppliers();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['product'] && this.product) {
      this.productForm.patchValue({
        name: this.product.name,
        sku: this.product.sku,
        description: this.product.description,
        categoryId: this.product.categoryId,
        supplierId: this.product.supplierId || '',
        costPrice: this.product.costPrice,
        salePrice: this.product.salePrice
      });
    } else if (changes['product'] && !this.product) {
      this.productForm.reset();
    }
  }

  loadCategories() {
    this.categoryService.getAllCategories().subscribe({
      next: (categories) => this.categories = categories,
      error: (error) => console.error('Erro ao carregar categorias:', error)
    });
  }

  loadSuppliers() {
    this.supplierService.getAllSuppliers().subscribe({
      next: (suppliers) => this.suppliers = suppliers,
      error: (error) => console.error('Erro ao carregar fornecedores:', error)
    });
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
      // Modo edição
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
      // Modo criação
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
