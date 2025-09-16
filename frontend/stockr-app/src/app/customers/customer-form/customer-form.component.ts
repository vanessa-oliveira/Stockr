import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Customer } from '../../models/customer';
import { CustomerService, CreateCustomerRequest, UpdateCustomerRequest } from '../../services/customer.service';
import { InputMaskModule } from 'primeng/inputmask';
import { RadioButtonModule } from 'primeng/radiobutton';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [
    DialogModule,
    ButtonModule,
    InputTextModule,
    ReactiveFormsModule,
    InputMaskModule,
    RadioButtonModule
  ],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss'
})
export class CustomerFormComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Input() customer: Customer | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() customerChanged = new EventEmitter<void>();

  public customerForm!: FormGroup;
  public isLoading = false;

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService
  ) {
    this.customerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      documentType: ['cpf'],
      cpf: [''],
      cnpj: ['']
    });
  }

  get documentType(): 'cpf' | 'cnpj' {
    return this.customerForm.get('documentType')?.value || 'cpf';
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['customer'] && this.customer) {
      const docType = this.customer.cpf ? 'cpf' : 'cnpj';
      this.customerForm.patchValue({
        name: this.customer.name,
        email: this.customer.email,
        phone: this.customer.phone,
        documentType: docType,
        cpf: this.customer.cpf || '',
        cnpj: this.customer.cnpj || ''
      });
    } else if (changes['customer'] && !this.customer) {
      this.customerForm.reset({ documentType: 'cpf' });
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
    if (this.customerForm.invalid) return;

    this.isLoading = true;
    const formValue = this.customerForm.value;

    const customerData = {
      name: formValue.name,
      email: formValue.email,
      phone: formValue.phone,
      cpf: this.documentType === 'cpf' ? formValue.cpf : undefined,
      cnpj: this.documentType === 'cnpj' ? formValue.cnpj : undefined
    };

    if (this.customer) {
      // Modo edição
      const updateRequest: UpdateCustomerRequest = {
        id: this.customer.id,
        ...customerData
      };

      this.customerService.updateCustomer(updateRequest).subscribe({
        next: () => {
          this.customerChanged.emit();
          this.closeDialog();
          this.customerForm.reset();
        },
        error: (error) => {
          console.error('Erro ao atualizar cliente:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      // Modo criação
      const createRequest: CreateCustomerRequest = customerData;

      this.customerService.addCustomer(createRequest).subscribe({
        next: () => {
          this.customerChanged.emit();
          this.closeDialog();
          this.customerForm.reset();
        },
        error: (error) => {
          console.error('Erro ao criar cliente:', error);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    }
  }
}
