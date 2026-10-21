export interface ApiError {
    statusCode?:number;
    errorCode?:string;
    message?:string;
    traceId?:string;
    errors?:Record<string,string[]>;
    title?:string;
}