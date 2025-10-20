import { Component } from '@angular/core';
import {TableLazyLoadEvent, TableModule} from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { Supplier } from '../../models/supplier';
import { SupplierService } from '../../services/supplier.service';
import { SupplierFormComponent } from '../supplier-form/supplier-form.component';
import { NgxMaskPipe, provideNgxMask } from 'ngx-mask';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    SupplierFormComponent,
    NgxMaskPipe
  ],
  providers: [ConfirmationService, provideNgxMask()],
  templateUrl: './supplier-list.component.html',
  styleUrl: './supplier-list.component.scss'
})
export class SupplierListComponent {
  public suppliers: Array<Supplier> = [];
  public visible: boolean = false;
  public selectedSupplier: Supplier | null = null;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  constructor(
    private supplierService: SupplierService,
    private confirmationService: ConfirmationService
  ) {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.supplierService.getAllSuppliers(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<Supplier>) => {
        this.suppliers = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Erro ao carregar fornecedores:', error)
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadSuppliers();
    }
  }

  public openSupplierForm() {
    this.selectedSupplier = null;
    this.visible = true;
  }

  public editSupplier(supplier: Supplier) {
    this.selectedSupplier = supplier;
    this.visible = true;
  }

  public deleteSupplier(supplier: Supplier) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o fornecedor "${supplier.name}"? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (supplier.id) {
          this.supplierService.deleteSupplier(supplier.id).subscribe({
            next: () => {
              this.loadSuppliers();
            },
            error: (error) => {
              console.error('Erro ao excluir fornecedor:', error);
            }
          });
        }
      }
    });
  }
}
