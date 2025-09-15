import {Component, Input, Output, EventEmitter, OnChanges, SimpleChanges} from '@angular/core';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {Category} from '../../models/category';
import {CategoryService} from '../../services/category.service';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [
    DialogModule,
    ButtonModule,
    InputTextModule,
    ReactiveFormsModule
  ],
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.scss'
})
export class CategoryFormComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Input() category: Category | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() categoryChanged = new EventEmitter<void>();

  public categoryForm!: FormGroup;
  public isLoading = false;

  constructor(private fb: FormBuilder, private categoryService: CategoryService) {
    this.categoryForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
    })
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['category'] && this.category) {
      this.categoryForm.patchValue({
        name: this.category.name,
        description: this.category.description
      });
    } else if (changes['category'] && !this.category) {
      this.categoryForm.reset();
    }
  }

  closeDialog() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  onVisibilityChange(visible: boolean) {
    this.visible = visible;
    this.visibleChange.emit(visible);
  }

  public onSubmit() {
    if (this.categoryForm.invalid) return;

    this.isLoading = true;
    const formValue = this.categoryForm.value;
    const category: Category = {
      name: formValue.name,
      description: formValue.description,
      ...(this.category?.id && { id: this.category.id })
    };

    const operation = this.category
      ? this.categoryService.updateCategory(category)
      : this.categoryService.addCategory(category);

    operation.subscribe({
      next: () => {
        this.categoryChanged.emit();
        this.closeDialog();
        this.categoryForm.reset();
      },
      error: (error) => {
        console.error('Erro ao salvar categoria:', error);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
