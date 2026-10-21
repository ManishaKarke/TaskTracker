import { Routes } from '@angular/router';
import { ProjectsComponent } from './features/projects/projects.component';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'projects',
    component: ProjectsComponent
  },
  {
    path: 'projects/:id/tasks',
    loadChildren: () =>
      import('./features/tasks/tasks.routes').then(m => m.TASK_ROUTES)
  },
  { path: '**', redirectTo: 'projects' }
];