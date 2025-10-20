import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TenantSignupRequest, PlanType } from '../../models/auth.model';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.scss'
})
export class SignupComponent {
  signupForm: FormGroup;
  loading = false;
  errorMessage = '';
  planTypes = [
    { value: PlanType.Basic, label: 'Básico', description: 'Ideal para começar' },
    { value: PlanType.Standard, label: 'Standard', description: 'Para negócios em crescimento' },
    { value: PlanType.Enterprise, label: 'Enterprise', description: 'Recursos completos' }
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.signupForm = this.fb.group({
      tenantName: ['', [Validators.required, Validators.minLength(3)]],
      planType: [PlanType.Basic, [Validators.required]],
      adminName: ['', [Validators.required, Validators.minLength(3)]],
      adminEmail: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) {
      return null;
    }

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.signupForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const request: TenantSignupRequest = {
      tenantName: this.signupForm.value.tenantName,
      planType: Number(this.signupForm.value.planType),
      adminName: this.signupForm.value.adminName,
      adminEmail: this.signupForm.value.adminEmail.toLowerCase().trim(),
      password: this.signupForm.value.password
    };

    this.authService.tenantSignup(request).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Erro ao criar conta. Tente novamente.';
        this.loading = false;
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  get tenantName() {
    return this.signupForm.get('tenantName');
  }

  get planType() {
    return this.signupForm.get('planType');
  }

  get adminName() {
    return this.signupForm.get('adminName');
  }

  get adminEmail() {
    return this.signupForm.get('adminEmail');
  }

  get password() {
    return this.signupForm.get('password');
  }

  get confirmPassword() {
    return this.signupForm.get('confirmPassword');
  }
}
