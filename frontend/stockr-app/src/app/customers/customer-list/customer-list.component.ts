import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { Customer } from '../../models/customer';
import { CustomerService } from '../../services/customer.service';
import { CustomerFormComponent } from '../customer-form/customer-form.component';
import { NgxMaskPipe, provideNgxMask } from 'ngx-mask';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    CustomerFormComponent,
    NgxMaskPipe
  ],
  providers: [ConfirmationService, provideNgxMask()],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss'
})
export class CustomerListComponent {
  public customers: Array<Customer> = [];
  public visible: boolean = false;
  public selectedCustomer: Customer | null = null;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  constructor(
    private customerService: CustomerService,
    private confirmationService: ConfirmationService
  ) {
    this.loadCustomers();
  }

  loadCustomers() {
    this.customerService.getCustomersPaged(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<Customer>) => {
        this.customers = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Erro ao carregar clientes:', error)
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadCustomers();
    }
  }

  public openCustomerForm() {
    this.selectedCustomer = null;
    this.visible = true;
  }

  public editCustomer(customer: Customer) {
    this.selectedCustomer = customer;
    this.visible = true;
  }

  public deleteCustomer(customer: Customer) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o cliente "${customer.name}"? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (customer.id) {
          this.customerService.deleteCustomer(customer.id).subscribe({
            next: () => {
              this.loadCustomers();
            },
            error: (error) => {
              console.error('Erro ao excluir cliente:', error);
            }
          });
        }
      }
    });
  }
}
