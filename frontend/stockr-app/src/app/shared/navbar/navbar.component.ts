import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User, UserRole } from '../../models/auth.model';
import {ButtonModule} from 'primeng/button';
import {Tooltip} from 'primeng/tooltip';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, ButtonModule, Tooltip],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  public checked: boolean = true;
  currentUser: User | null = null;
  UserRole = UserRole;

  constructor(private authService: AuthService) {
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  logout(): void {
    this.authService.logout();
  }

  isTenantAdmin(): boolean {
    return this.currentUser?.role === UserRole.TenantAdmin ||
           this.currentUser?.role === UserRole.SystemAdmin;
  }
}
