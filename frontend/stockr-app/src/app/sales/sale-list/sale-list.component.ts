import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { Sale, SaleStatus } from '../../models/sale';
import { SaleService } from '../../services/sale.service';
import { SaleFormComponent } from '../sale-form/sale-form.component';
import { CommonModule } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-sale-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    SaleFormComponent,
    TagModule
  ],
  providers: [ConfirmationService],
  templateUrl: './sale-list.component.html',
  styleUrl: './sale-list.component.scss'
})
export class SaleListComponent {
  public sales: Array<Sale> = [];
  public visible: boolean = false;
  public selectedSale: Sale | null = null;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  constructor(
    private saleService: SaleService,
    private confirmationService: ConfirmationService
  ) {
    this.loadSales();
  }

  loadSales() {
    this.saleService.getAllSales(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<Sale>) => {
        this.sales = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Erro ao carregar vendas:', error)
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadSales();
    }
  }

  public openSaleForm() {
    this.selectedSale = null;
    this.visible = true;
  }

  public editSale(sale: Sale) {
    this.selectedSale = sale;
    this.visible = true;
  }

  public deleteSale(sale: Sale) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a venda #${sale.id.substring(0, 8)}? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (sale.id) {
          this.saleService.deleteSale(sale.id).subscribe({
            next: () => {
              this.loadSales();
            },
            error: (error) => {
              console.error('Erro ao excluir venda:', error);
            }
          });
        }
      }
    });
  }

  public getSaleStatusLabel(status: SaleStatus): string {
    const labels: Record<SaleStatus, string> = {
      [SaleStatus.Draft]: 'Rascunho',
      [SaleStatus.Pending]: 'Pendente',
      [SaleStatus.Confirmed]: 'Confirmada',
      [SaleStatus.PendingPayment]: 'Aguardando Pagamento',
      [SaleStatus.Paid]: 'Pago',
      [SaleStatus.Completed]: 'Finalizada',
      [SaleStatus.Cancelled]: 'Cancelada'
    };
    return labels[status] || 'Desconhecido';
  }

  public getSaleStatusSeverity(status: SaleStatus): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    const severities: Record<SaleStatus, 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast'> = {
      [SaleStatus.Draft]: 'secondary',
      [SaleStatus.Pending]: 'warn',
      [SaleStatus.Confirmed]: 'info',
      [SaleStatus.PendingPayment]: 'warn',
      [SaleStatus.Paid]: 'info',
      [SaleStatus.Completed]: 'success',
      [SaleStatus.Cancelled]: 'danger'
    };
    return severities[status] || 'secondary';
  }
}
