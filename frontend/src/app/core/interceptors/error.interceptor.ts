import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

const ERROR_MESSAGES: Record<number, string> = {
  400: 'Solicitud inválida. Verifica los datos ingresados.',
  401: 'No autorizado. Por favor, inicia sesión.',
  403: 'No tienes permisos para realizar esta acción.',
  404: 'El recurso solicitado no fue encontrado.',
  409: '',
  422: 'Operación no permitida en el estado actual.',
  500: 'Error interno del servidor. Intenta más tarde.',
};

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 409 (idempotencia) no se muestra como error
      if (error.status === 409) {
        return throwError(() => error);
      }

      const message =
        ERROR_MESSAGES[error.status] ??
        error.error?.detail ??
        'Ocurrió un error inesperado.';

      if (message) {
        snackBar.open(message, 'Cerrar', {
          duration: 5000,
          panelClass: ['snack-error'],
        });
      }

      return throwError(() => error);
    })
  );
};
