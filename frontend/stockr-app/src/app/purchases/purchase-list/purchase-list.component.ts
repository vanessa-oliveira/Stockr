import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { Purchase } from '../../models/purchase';
import { PurchaseService } from '../../services/purchase.service';
import { PurchaseFormComponent } from '../purchase-form/purchase-form.component';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    PurchaseFormComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './purchase-list.component.html',
  styleUrl: './purchase-list.component.scss'
})
export class PurchaseListComponent {
  public purchases: Array<Purchase> = [];
  public visible: boolean = false;
  public selectedPurchase: Purchase | null = null;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  constructor(
    private purchaseService: PurchaseService,
    private confirmationService: ConfirmationService
  ) {
    this.loadPurchases();
  }

  loadPurchases() {
    this.purchaseService.getAllPurchases(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<Purchase>) => {
        this.purchases = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Erro ao carregar compras:', error)
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadPurchases();
    }
  }

  public openPurchaseForm() {
    this.selectedPurchase = null;
    this.visible = true;
  }

  public editPurchase(purchase: Purchase) {
    this.selectedPurchase = purchase;
    this.visible = true;
  }

  public deletePurchase(purchase: Purchase) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a compra "${purchase.invoiceNumber}"? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (purchase.id) {
          this.purchaseService.deletePurchase(purchase.id).subscribe({
            next: () => {
              this.loadPurchases();
            },
            error: (error) => {
              console.error('Erro ao excluir compra:', error);
            }
          });
        }
      }
    });
  }
}
