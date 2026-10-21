import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Project, ProjectRequest } from '../../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/projects`;

  getAll():Observable<Project[]>{
    return this.http.get<Project[]>(this.apiUrl);
  }

  getById(id:number):Observable<Project>{
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }
   
  create(request: ProjectRequest): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, request);
  }

  update(id: number, request: ProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
