import { HttpInterceptorFn } from '@angular/common/http';

export const idempotencyInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.method === 'POST') {
    const idempotencyKey = crypto.randomUUID();
    req = req.clone({ setHeaders: { 'Idempotency-Key': idempotencyKey } });
  }
  return next(req);
};
