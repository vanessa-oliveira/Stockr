import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Supplier, CreateSupplierRequest, UpdateSupplierRequest } from '../../models/supplier';
import { SupplierService } from '../../services/supplier.service';
import { InputMaskModule } from 'primeng/inputmask';

@Component({
  selector: 'app-supplier-form',
  standalone: true,
  imports: [
    DialogModule,
    ButtonModule,
    InputTextModule,
    ReactiveFormsModule,
    InputMaskModule
  ],
  templateUrl: './supplier-form.component.html',
  styleUrl: './supplier-form.component.scss'
})
export class SupplierFormComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Input() supplier: Supplier | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() supplierChanged = new EventEmitter<void>();

  public supplierForm!: FormGroup;
  public isLoading = false;

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService
  ) {
    this.supplierForm = this.fb.group({
      name: ['', Validators.required],
      phone: ['', Validators.required]
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['supplier'] && this.supplier) {
      this.supplierForm.patchValue({
        name: this.supplier.name,
        phone: this.supplier.phone
      });
    } else if (changes['supplier'] && !this.supplier) {
      this.supplierForm.reset();
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
    if (this.supplierForm.invalid) return;

    this.isLoading = true;
    const formValue = this.supplierForm.value;

    if (this.supplier) {
      // Modo edição
      const updateRequest: UpdateSupplierRequest = {
        id: this.supplier.id,
        name: formValue.name,
        phone: formValue.phone
      };

      this.supplierService.updateSupplier(updateRequest).subscribe({
        next: () => {
          this.supplierChanged.emit();
          this.closeDialog();
          this.supplierForm.reset();
        },
        error: (error) => {
          console.error('Erro ao atualizar fornecedor:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      // Modo criação
      const createRequest: CreateSupplierRequest = {
        name: formValue.name,
        phone: formValue.phone
      };

      this.supplierService.addSupplier(createRequest).subscribe({
        next: () => {
          this.supplierChanged.emit();
          this.closeDialog();
          this.supplierForm.reset();
        },
        error: (error) => {
          console.error('Erro ao criar fornecedor:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    }
  }
}
