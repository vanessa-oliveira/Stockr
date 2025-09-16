import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { Customer } from '../../models/customer';
import { CustomerService } from '../../services/customer.service';
import { CustomerFormComponent } from '../customer-form/customer-form.component';
import { NgxMaskPipe, provideNgxMask } from 'ngx-mask';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
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

  constructor(
    private customerService: CustomerService,
    private confirmationService: ConfirmationService
  ) {
    this.loadCustomers();
  }

  loadCustomers() {
    this.customerService.getAllCustomers().subscribe({
      next: (result) => this.customers = result,
      error: (error) => console.error('Erro ao carregar clientes:', error)
    });
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