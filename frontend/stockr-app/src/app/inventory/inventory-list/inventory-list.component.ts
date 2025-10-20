import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { Inventory } from '../../models/inventory';
import { InventoryService } from '../../services/inventory.service';
import { InventoryFormComponent } from '../inventory-form/inventory-form.component';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    InventoryFormComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './inventory-list.component.html',
  styleUrl: './inventory-list.component.scss'
})
export class InventoryListComponent {
  public inventory: Array<Inventory> = [];
  public visible: boolean = false;
  public selectedInventory: Inventory | null = null;

  public pageNumber: number = 1;
  public pageSize: number = 10;
  public totalCount: number = 0;

  constructor(
    private inventoryService: InventoryService,
    private confirmationService: ConfirmationService
  ) {
    this.loadInventory();
  }

  loadInventory() {
    this.inventoryService.getAllInventory(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<Inventory>) => {
        this.inventory = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Erro ao carregar estoque:', error)
    });
  }

  pageChange(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 10;
    const page = Math.floor(first / rows) + 1;

    if (this.pageNumber !== page || this.pageSize !== rows) {
      this.pageNumber = page;
      this.pageSize = rows;
      this.loadInventory();
    }
  }

  public openInventoryForm() {
    this.selectedInventory = null;
    this.visible = true;
  }

  public editInventory(inventory: Inventory) {
    this.selectedInventory = inventory;
    this.visible = true;
  }

  public deleteInventory(inventory: Inventory) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a configuração de estoque do produto "${inventory.productName}"? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (inventory.id) {
          this.inventoryService.deleteInventory(inventory.id).subscribe({
            next: () => {
              this.loadInventory();
            },
            error: (error) => {
              console.error('Erro ao excluir estoque:', error);
            }
          });
        }
      }
    });
  }
}
