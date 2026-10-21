import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../../models/api-error.model';

export const apiErrorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const body = error.error as ApiError | string | null;
      let message = 'Something went wrong. Please try again.';

      if (typeof body === 'object' && body !== null) {
        message = body.message || body.title || message;

        if (body.errors) {
          const validationMessages = Object.values(body.errors).flat();
          if (validationMessages.length) {
            message = validationMessages.join('\n');
          }
        }
      } else if (typeof body === 'string' && body.trim()) {
        message = body;
      }

      if (error.status === 0) {
        message = 'Cannot connect to the API. Check that the backend is running.';
      } else if (error.status === 409) {
        message = message || 'This record was changed by another user. Refresh and try again.';
      }

      window.alert(message);

      return throwError(() => error);
    })
  );
};