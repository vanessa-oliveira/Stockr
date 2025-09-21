import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { Inventory } from '../../models/inventory';
import { InventoryService } from '../../services/inventory.service';
import { InventoryFormComponent } from '../inventory-form/inventory-form.component';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
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

  constructor(
    private inventoryService: InventoryService,
    private confirmationService: ConfirmationService
  ) {
    this.loadInventory();
  }

  loadInventory() {
    this.inventoryService.getAllInventory().subscribe({
      next: (result) => this.inventory = result,
      error: (error) => console.error('Erro ao carregar estoque:', error)
    });
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