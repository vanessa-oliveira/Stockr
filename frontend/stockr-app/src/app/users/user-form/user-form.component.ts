import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { UserRole } from '../../models/auth.model';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputMaskModule } from 'primeng/inputmask';
import { PasswordModule } from 'primeng/password';
import {CreateUserRequest} from '../../models/user.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    InputTextModule,
    ButtonModule,
    SelectModule,
    InputMaskModule,
    PasswordModule
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() userChanged = new EventEmitter<void>();

  createUserForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';

  availableRoles = [
    { value: UserRole.Manager, label: 'Gerente', description: 'Acesso total às operações' },
    { value: UserRole.Seller, label: 'Vendedor', description: 'Vendas e consultas' },
    { value: UserRole.StockController, label: 'Estoquista', description: 'Gestão de estoque' },
    { value: UserRole.Cashier, label: 'Operador de Caixa', description: 'Vendas básicas' },
    { value: UserRole.Viewer, label: 'Visualizador', description: 'Apenas consultas' }
  ];

  constructor(
    private userService: UserService,
    private fb: FormBuilder
  ) {
    this.createUserForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      role: [UserRole.Viewer, [Validators.required]]
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && changes['visible'].currentValue) {
      this.resetForm();
    }
  }

  resetForm(): void {
    this.createUserForm.reset({ role: UserRole.Viewer });
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeDialog(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  onCreateUser(): void {
    if (this.createUserForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateUserRequest = {
      name: this.createUserForm.value.name,
      email: this.createUserForm.value.email.toLowerCase().trim(),
      password: this.createUserForm.value.password,
      role: this.createUserForm.value.role
    };

    this.userService.createUser(request).subscribe({
      next: () => {
        this.successMessage = 'Usuário criado com sucesso!';
        this.loading = false;
        this.userChanged.emit();

        setTimeout(() => {
          this.closeDialog();
        }, 1500);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Erro ao criar usuário';
        this.loading = false;
      }
    });
  }

  get name() {
    return this.createUserForm.get('name');
  }

  get email() {
    return this.createUserForm.get('email');
  }

  get password() {
    return this.createUserForm.get('password');
  }

  get role() {
    return this.createUserForm.get('role');
  }
}
