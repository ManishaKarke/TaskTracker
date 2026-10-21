import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ProjectService } from '../../core/services/project.service';
import { Project, ProjectRequest } from '../../models/project.model';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css'
})
export class ProjectsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);

  projects: Project[] = [];
  loading = false;
  saving = false;
  formVisible = false;
  editingProject: Project | null = null;

  projectForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', Validators.maxLength(500)]
  });

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.loading = true;

    this.projectService.getAll()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: projects => this.projects = projects
      });
  }

  openCreateForm(): void {
    this.editingProject = null;
    this.projectForm.reset({ name: '', description: '' });
    this.formVisible = true;
  }

  openEditForm(project: Project): void {
    this.editingProject = project;
    this.projectForm.reset({
      name: project.name,
      description: project.description ?? ''
    });
    this.formVisible = true;
  }

  saveProject(): void {
    if (this.projectForm.invalid || this.saving) {
      this.projectForm.markAllAsTouched();
      return;
    }

    const value = this.projectForm.getRawValue();
    const request: ProjectRequest = {
      name: value.name.trim(),
      description: value.description.trim() || null,
      rowVersion: ''
    };

    if (this.editingProject) {
      request.rowVersion = this.editingProject.rowVersion;
    }

    this.saving = true;

    const operation = this.editingProject
      ? this.projectService.update(this.editingProject.id, request)
      : this.projectService.create(request);

    operation.pipe(finalize(() => this.saving = false))
      .subscribe({
        next: () => {
          this.formVisible = false;
          this.loadProjects();
        }
      });
  }

  deleteProject(project: Project): void {
    const confirmed = window.confirm(
      `Delete "${project.name}" and all its tasks?`
    );

    if (!confirmed) {
      return;
    }

    this.projectService.delete(project.id).subscribe({
      next: () => this.loadProjects()
    });
  }

  openTasks(project: Project): void {
    this.router.navigate(['/projects', project.id, 'tasks']);
  }

  get nameControl() {
    return this.projectForm.controls.name;
  }

  get descriptionControl() {
    return this.projectForm.controls.description;
  }
}