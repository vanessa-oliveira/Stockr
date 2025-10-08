import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import {Category} from '../../models/category';
import {CategoryService} from '../../services/category.service';
import { CategoryFormComponent } from '../category-form/category-form.component';
import { PagedResult } from '../../models/pagination.model';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    ConfirmDialogModule,
    TooltipModule,
    CategoryFormComponent
  ],
  providers: [ConfirmationService],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.scss'
})
export class CategoryListComponent {
    public categories: Array<Category> = [];
    public visible: boolean = false;
    public selectedCategory: Category | null = null;

    public pageNumber: number = 1;
    public pageSize: number = 10;
    public totalCount: number = 0;

    constructor(
      private categoryService: CategoryService,
      private confirmationService: ConfirmationService
    ) {
      this.loadCategories();
    }

    loadCategories() {
      this.categoryService.getCategoriesPaged(this.pageNumber, this.pageSize).subscribe({
        next: (result: PagedResult<Category>) => {
          this.categories = result.items;
          this.totalCount = result.totalCount;
        },
        error: (error) => console.error('Erro ao carregar categorias:', error)
      });
    }

    pageChange(event: any): void {
      const first = event.first ?? 0;
      const rows = event.rows ?? 10;
      const page = Math.floor(first / rows) + 1;

      if (this.pageNumber !== page || this.pageSize !== rows) {
        this.pageNumber = page;
        this.pageSize = rows;
        this.loadCategories();
      }
    }

  public openCategoryForm() {
    this.selectedCategory = null;
    this.visible = true;
  }

  public editCategory(category: Category) {
    this.selectedCategory = category;
    this.visible = true;
  }

  public deleteCategory(category: Category) {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a categoria "${category.name}"? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      accept: () => {
        if (category.id) {
          this.categoryService.deleteCategory(category.id).subscribe({
            next: () => {
              this.loadCategories();
            },
            error: (error) => {
              console.error('Erro ao excluir categoria:', error);
            }
          });
        }
      }
    });
  }
}
