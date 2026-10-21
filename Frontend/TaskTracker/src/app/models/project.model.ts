export interface Project{
    id: number;
    name: string;
    description: string|null;
    createdAt:string;
    taskCount:number;
    rowVersion:string;
}

export interface ProjectRequest {
    name: string;
    description: string|null;
    rowVersion:string;
}