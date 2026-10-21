export enum TaskStatus{
    Todo = 0,
    InProgress = 1,
    Done = 2
}

export enum TaskPriority{
    Low = 0,
    Medium = 1,
    High = 2
}

export interface TaskItem{
    id: number;
    projectId: number;
    description:string|null;
    status:TaskStatus;
    priority:TaskPriority;
    assignedTo:string|null;
    dueDate:string|null;
    createdAt:string;
    rowVersion:string;
}

export interface TaskQuery{
    status?: TaskStatus;
    priority?:TaskPriority;
    search?:string;
    sortBy: 'dueDate'|'priority'|'createdAt';
    sortDir:'asc'|'desc';
    page:number;
    pageSize:number;
}

export interface PagedResponse<T>{
    item: T[];
    page:number;
    pageSize:number;
    totalCount:number;
}