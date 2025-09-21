import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { Purchase } from '../../models/purchase';
import { PurchaseService } from '../../services/purchase.service';
import { PurchaseFormComponent } from '../purchase-form/purchase-form.component';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
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

  constructor(
    private purchaseService: PurchaseService,
    private confirmationService: ConfirmationService
  ) {
    this.loadPurchases();
  }

  loadPurchases() {
    this.purchaseService.getAllPurchases().subscribe({
      next: (result) => this.purchases = result,
      error: (error) => console.error('Erro ao carregar compras:', error)
    });
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