import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { PagedResponse, TaskItem, TaskQuery, TaskRequest, TaskStatus } from '../../models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}`;

  getByProject(projectId:number, query:TaskQuery):Observable<PagedResponse<TaskItem>>
  {
    let params = new HttpParams().set('sortBy',query.sortBy).set('sortDir',query.sortDir).set('page',query.page).set('pageSize',query.pageSize);

    if(query.status !== undefined)
    {
      params = params.set('status',query.status);
    }

    if(query.priority !== undefined)
    {
        params = params.set('priority',query.priority);
    }

    if(query.search?.trim())
    {
          params = params.set('search',query.search.trim());
    }

    return this.http.get<PagedResponse<TaskItem>>(`${this.apiUrl}/projects/${projectId}/tasks`,{params});
  }

  getById(id:number): Observable<TaskItem>{
    return this.http.get<TaskItem>(`${this.apiUrl}/tasks/${id}`);
  }

  create(projectId: number, request: TaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(
      `${this.apiUrl}/projects/${projectId}/tasks`,
      request
    );
  }

  update(id: number, request: TaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(
      `${this.apiUrl}/tasks/${id}`,
      request
    );
  }

  updateStatus(
    id: number,
    status: TaskStatus,
    rowVersion: string
  ): Observable<TaskItem> {
    return this.http.patch<TaskItem>(
      `${this.apiUrl}/tasks/${id}/status`,
      { status, rowVersion }
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tasks/${id}`);
  }

}
