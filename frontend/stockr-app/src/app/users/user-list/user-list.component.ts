import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { UserRole } from '../../models/auth.model';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { UserFormComponent } from '../user-form/user-form.component';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { PagedResult } from '../../models/pagination.model';
import {User} from '../../models/user.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, ButtonModule, TooltipModule, TableModule, UserFormComponent, ConfirmDialogModule],
  providers: [ConfirmationService],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit {
  users: User[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  showCreateForm = false;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  availableRoles = [
    { value: UserRole.Manager, label: 'Gerente', description: 'Acesso total às operações' },
    { value: UserRole.Seller, label: 'Vendedor', description: 'Vendas e consultas' },
    { value: UserRole.StockController, label: 'Estoquista', description: 'Gestão de estoque' },
    { value: UserRole.Cashier, label: 'Operador de Caixa', description: 'Vendas básicas' },
    { value: UserRole.Viewer, label: 'Visualizador', description: 'Apenas consultas' }
  ];

  constructor(private userService: UserService, private confirmationService: ConfirmationService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = '';

    this.userService.getUsersPaged(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<User>) => {
        this.users = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Erro ao carregar usuários';
        this.loading = false;
      }
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadUsers();
    }
  }

  openUserForm(): void {
    this.showCreateForm = true;
  }

  onBlockUser(user: User): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja bloquear o usuário ${user.name}?`,
      header: 'Confirmar Bloqueio',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, bloquear',
      rejectLabel: 'Cancelar',
      accept: () => {
        const blockedUntil = new Date();
        blockedUntil.setHours(blockedUntil.getHours() + 24);

        this.userService.blockUser(user.id, blockedUntil).subscribe({
          next: () => {
            this.successMessage = 'Usuário bloqueado com sucesso!';
            this.loadUsers();
            setTimeout(() => {
              this.successMessage = '';
            }, 3000);
          },
          error: (error) => {
            this.errorMessage = error.error?.message || 'Erro ao bloquear usuário';
          }
        });
      }
    });
  }

  onUnblockUser(user: User): void {
    this.userService.unblockUser(user.id).subscribe({
      next: () => {
        this.successMessage = 'Usuário desbloqueado com sucesso!';
        this.loadUsers();
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Erro ao desbloquear usuário';
      }
    });
  }

  onDeleteUser(user: User): void {
    if (!confirm(`Deseja realmente excluir o usuário ${user.name}? Esta ação não pode ser desfeita.`)) {
      return;
    }

    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.successMessage = 'Usuário excluído com sucesso!';
        this.loadUsers();
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Erro ao excluir usuário';
      }
    });
  }

  getRoleLabel(role: UserRole): string {
    const roleItem = this.availableRoles.find(r => r.value === role);
    return roleItem?.label || 'Usuário';
  }

  getRoleBadgeClass(role: UserRole): string {
    switch (role) {
      case UserRole.Manager:
        return 'badge-manager';
      case UserRole.Seller:
        return 'badge-seller';
      case UserRole.StockController:
        return 'badge-stock';
      case UserRole.Cashier:
        return 'badge-cashier';
      case UserRole.Viewer:
        return 'badge-viewer';
      default:
        return 'badge-default';
    }
  }

  getRoleIcon(role: UserRole): string {
    switch (role) {
      case UserRole.Manager:
        return 'pi pi-crown';
      case UserRole.Seller:
        return 'pi pi-shopping-bag';
      case UserRole.StockController:
        return 'pi pi-box';
      case UserRole.Cashier:
        return 'pi pi-dollar';
      case UserRole.Viewer:
        return 'pi pi-eye';
      default:
        return 'pi pi-user';
    }
  }
}
