
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  BehaviorSubject,
  EMPTY,
  catchError,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  finalize,
  startWith,
  switchMap,
  tap
} from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { TaskService } from '../../core/services/task.service';
import { ProjectService } from '../../core/services/project.service';
import {
  TaskItem,
  TaskPriority,
  TaskQuery,
  TaskRequest,
  TaskStatus,
  PagedResponse
} from '../../models/task.model';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './tasks.component.html',
  styleUrl: './tasks.component.css'
})
export class TasksComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly taskService = inject(TaskService);
  private readonly projectService = inject(ProjectService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  projectId = 0;
  project: Project | null = null;
  tasks: TaskItem[] = [];

  loading = false;
  saving = false;
  formVisible = false;
  editingTask: TaskItem | null = null;

  page = 1;
  pageSize = 10;
  totalCount = 0;

  sortBy: TaskQuery['sortBy'] = 'createdAt';
  sortDir: 'asc' | 'desc' = 'desc';

  readonly statuses = [
    { value: TaskStatus.Todo, label: 'To Do' },
    { value: TaskStatus.InProgress, label: 'In Progress' },
    { value: TaskStatus.Done, label: 'Done' }
  ];

  readonly priorities = [
    { value: TaskPriority.Low, label: 'Low' },
    { value: TaskPriority.Medium, label: 'Medium' },
    { value: TaskPriority.High, label: 'High' }
  ];

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly statusFilter = new FormControl('');
  readonly priorityFilter = new FormControl('');

  private readonly refresh$ = new BehaviorSubject<void>(undefined);

  readonly taskForm = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', Validators.maxLength(1000)],
    status: [TaskStatus.Todo, Validators.required],
    priority: [TaskPriority.Medium, Validators.required],
    assignedTo: ['', Validators.maxLength(100)],
    dueDate: ['']
  });

  ngOnInit(): void {
    this.projectId = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(this.projectId) || this.projectId <= 0) {
      window.alert('Invalid project ID.');
      return;
    }

    this.projectService.getById(this.projectId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: project => {
          this.project = project;
        },
        error: error => {
          console.error('Failed to load project:', error);
          window.alert('Unable to load project details.');
        }
      });

    const search$ = this.searchControl.valueChanges.pipe(
      startWith(this.searchControl.value),
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => {
        this.page = 1;
      })
    );

    const filters$ = combineLatest([
      this.statusFilter.valueChanges.pipe(
        startWith(this.statusFilter.value)
      ),
      this.priorityFilter.valueChanges.pipe(
        startWith(this.priorityFilter.value)
      )
    ]).pipe(
      distinctUntilChanged(
        (previous, current) =>
          previous[0] === current[0] &&
          previous[1] === current[1]
      ),
      tap(() => {
        this.page = 1;
      })
    );

    combineLatest([search$, filters$, this.refresh$]).pipe(
      switchMap(([search, filters]) => {
        this.loading = true;

        const [statusValue, priorityValue] = filters;

        const query: TaskQuery = {
          search: search.trim(),
          status: statusValue === ''
            ? undefined
            : Number(statusValue) as TaskStatus,
          priority: priorityValue === ''
            ? undefined
            : Number(priorityValue) as TaskPriority,
          sortBy: this.sortBy,
          sortDir: this.sortDir,
          page: this.page,
          pageSize: this.pageSize
        };

        return this.taskService.getByProject(this.projectId, query).pipe(
          catchError(error => {
            console.error('Failed to load tasks:', error);
            this.tasks = [];
            this.totalCount = 0;

            window.alert(
              'Unable to load tasks. Check the API and try again.'
            );

            return EMPTY;
          }),
          finalize(() => {
            this.loading = false;
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((result: PagedResponse<TaskItem>) => {
      this.tasks = Array.isArray(result.items) ? result.items : [];
      this.page = result.page ?? 1;
      this.pageSize = result.pageSize ?? 10;
      this.totalCount = result.totalCount ?? this.tasks.length;

      console.log('Tasks loaded:', this.tasks.length);
      console.log('Total matching tasks:', this.totalCount);
    });
  }

  getStatusLabel(status: number): string {
    return this.statuses.find(item => item.value === status)?.label
      ?? 'Unknown Status';
  }

  getPriorityLabel(priority: number): string {
    return this.priorities.find(item => item.value === priority)?.label
      ?? 'Unknown Priority';
  }

  openCreateForm(): void {
    this.editingTask = null;

    this.taskForm.reset({
      title: '',
      description: '',
      status: TaskStatus.Todo,
      priority: TaskPriority.Medium,
      assignedTo: '',
      dueDate: ''
    });

    this.formVisible = true;
  }

  openEditForm(task: TaskItem): void {
    this.editingTask = task;

    this.taskForm.reset({
      title: task.title,
      description: task.description ?? '',
      status: task.status,
      priority: task.priority,
      assignedTo: task.assignedTo ?? '',
      dueDate: task.dueDate ? task.dueDate.substring(0, 10) : ''
    });

    this.formVisible = true;
  }

  saveTask(): void {
    if (this.taskForm.invalid || this.saving) {
      this.taskForm.markAllAsTouched();
      return;
    }

    const value = this.taskForm.getRawValue();

    const request: TaskRequest = {
      title: value.title!.trim(),
      description: value.description?.trim() || null,
      status: value.status!,
      priority: value.priority!,
      assignedTo: value.assignedTo?.trim() || null,
      dueDate: value.dueDate || null
    };

    if (this.editingTask) {
      request.rowVersion = this.editingTask.rowversion;
    }

    this.saving = true;

    const operation = this.editingTask
      ? this.taskService.update(this.editingTask.id, request)
      : this.taskService.create(this.projectId, request);

    operation.pipe(
      finalize(() => {
        this.saving = false;
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.formVisible = false;
        this.refresh$.next();
      },
      error: error => {
        console.error('Failed to save task:', error);
        window.alert(
          'Unable to save the task. Please check the entered data.'
        );
      }
    });
  }

  changeStatus(task: TaskItem, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const status = Number(select.value) as TaskStatus;

    this.taskService.updateStatus(task.id, status, task.rowversion)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.refresh$.next();
        },
        error: error => {
          console.error('Failed to update task status:', error);
          window.alert(
            'Unable to update status. The task list will be refreshed.'
          );
          this.refresh$.next();
        }
      });
  }

  deleteTask(task: TaskItem): void {
    if (!window.confirm(`Delete task "${task.title}"?`)) {
      return;
    }

    this.taskService.delete(task.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          if (this.tasks.length === 1 && this.page > 1) {
            this.page--;
          }

          this.refresh$.next();
        },
        error: error => {
          console.error('Failed to delete task:', error);
          window.alert('Unable to delete the task. Please try again.');
        }
      });
  }

  changeSort(value: string): void {
    this.sortBy = value as TaskQuery['sortBy'];
    this.page = 1;
    this.refresh$.next();
  }

  changeDirection(value: string): void {
    this.sortDir = value as 'asc' | 'desc';
    this.page = 1;
    this.refresh$.next();
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.page = newPage;
      this.refresh$.next();
    }
  }

  readonly minDueDate = this.getTodayDate();

private getTodayDate(): string {
  const today = new Date();

  // Use the user's local date rather than UTC.
  const year = today.getFullYear();
  const month = String(today.getMonth() + 1).padStart(2, '0');
  const day = String(today.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}

  changeStatusValue(task: TaskItem, value: number): void {
    const status = Number(value) as TaskStatus;
  
    this.taskService
      .updateStatus(task.id, status, task.rowversion)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.refresh$.next();
        },
        error: error => {
          console.error('Failed to update task status:', error);
          window.alert('Unable to update task status.');
          this.refresh$.next();
        }
      });
  }

  goToProjects(): void {
    this.router.navigate(['/projects']);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get titleControl() {
    return this.taskForm.controls.title;
  }
}
