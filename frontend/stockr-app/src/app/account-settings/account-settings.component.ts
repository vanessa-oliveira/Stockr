import {Component, OnInit} from '@angular/core';
import { PanelModule } from 'primeng/panel';
import {AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ImageModule } from 'primeng/image';
import {NgClass} from '@angular/common';
import {Button} from 'primeng/button';
import {ChangeUserPasswordRequest} from '../models/user.model';
import {UserService} from '../services/user.service';
import {ActivatedRoute, RouterModule} from '@angular/router';

@Component({
  selector: 'app-account-settings',
  standalone: true,
  imports: [
    PanelModule,
    ReactiveFormsModule,
    InputTextModule,
    ImageModule,
    NgClass,
    Button,
    RouterModule
  ],
  templateUrl: './account-settings.component.html',
  styleUrl: './account-settings.component.scss'
})
export class AccountSettingsComponent {
  personalInfoForm: FormGroup;
  passwordChangeForm: FormGroup;
  expanded: boolean = true;
  expandedPasswordPanel: boolean = true;
  userId: string;

  // Password visibility toggles
  showCurrentPassword: boolean = false;
  showNewPassword: boolean = false;
  showConfirmNewPassword: boolean = false;

  constructor(private formBuilder: FormBuilder, private userService: UserService, private route: ActivatedRoute) {
    this.route.params.subscribe(params => {
      this.userId = params['id'];
    })
    this.personalInfoForm = this.formBuilder.group({
      name: ['', Validators.required],
      email: ['', Validators.required],
    })
    this.passwordChangeForm = this.formBuilder.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', Validators.required, Validators.minLength(8)],
      confirmNewPassword: ['', Validators.required],
    }, {
      validators: this.passwordMatchValidator
    })
  }

  get currentPassword() {
    return this.passwordChangeForm.get('currentPassword');
  }

  get newPassword() {
    return this.passwordChangeForm.get('newPassword');
  }

  get confirmNewPassword() {
    return this.passwordChangeForm.get('confirmNewPassword');
  }

  passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const newPassword = control.get('newPassword');
    const confirmNewPassword = control.get('confirmNewPassword');

    if (!newPassword || !confirmNewPassword) {
      return null;
    }

    return newPassword.value === confirmNewPassword.value ? null : { passwordMismatch: true };
  }

  updatePersonalInfo(){

  }

  toggleCurrentPassword() {
    this.showCurrentPassword = !this.showCurrentPassword;
  }

  toggleNewPassword() {
    this.showNewPassword = !this.showNewPassword;
  }

  toggleConfirmNewPassword() {
    this.showConfirmNewPassword = !this.showConfirmNewPassword;
  }

  changePassword(){
    let formValue = this.passwordChangeForm.value;
    const request: ChangeUserPasswordRequest = {
      id: this.userId,
      currentPassword: formValue.currentPassword,
      newPassword: formValue.newPassword,
    }
    this.userService.changePassword(this.userId, request).subscribe();
  }
}
