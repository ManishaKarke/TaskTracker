
export enum TaskStatus {
  Todo = 0,
  InProgress = 1,
  Done = 2
}


export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2
}


export interface TaskItem {
  id: number;
  projectId: number;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  assignedTo: string | null;
  dueDate: string | null;
  createdAt: string;

  // The current API response serializes this property as "rowversion".
  rowversion: string;
}

export interface TaskRequest {
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  assignedTo: string | null;
  dueDate: string | null;
  rowVersion?: string;
}

export interface TaskQuery {
  status?: TaskStatus;
  priority?: TaskPriority;
  search?: string;
  sortBy: 'dueDate' | 'priority' | 'createdAt';
  sortDir: 'asc' | 'desc';
  page: number;
  pageSize: number;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  pageCount?: number;
}
