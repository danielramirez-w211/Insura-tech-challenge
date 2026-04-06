import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should pass through successful responses without calling snackBar', () => {
    http.get('/api/test').subscribe();
    const req = httpMock.expectOne('/api/test');
    req.flush({ data: 'ok' });
    expect(snackBarSpy.open).not.toHaveBeenCalled();
  });

  it('should show "El recurso solicitado no fue encontrado." on 404', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Not Found', { status: 404, statusText: 'Not Found' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'El recurso solicitado no fue encontrado.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should show "Error interno del servidor. Intenta más tarde." on 500', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Error interno del servidor. Intenta más tarde.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should NOT call snackBar on 409 (idempotency conflict)', () => {
    http.post('/api/test', {}).subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Conflict', { status: 409, statusText: 'Conflict' });
    expect(snackBarSpy.open).not.toHaveBeenCalled();
  });

  it('should show "Solicitud inválida. Verifica los datos ingresados." on 400', () => {
    http.post('/api/test', {}).subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Solicitud inválida. Verifica los datos ingresados.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should show "No autorizado. Por favor, inicia sesión." on 401', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'No autorizado. Por favor, inicia sesión.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should show "No tienes permisos para realizar esta acción." on 403', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'No tienes permisos para realizar esta acción.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should show "Operación no permitida en el estado actual." on 422', () => {
    http.put('/api/test', {}).subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Unprocessable', { status: 422, statusText: 'Unprocessable Entity' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Operación no permitida en el estado actual.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should show error.error.detail for unknown status codes when detail is available', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush({ detail: 'Custom error detail' }, { status: 418, statusText: "I'm a teapot" });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Custom error detail',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should show "Ocurrió un error inesperado." for unknown status codes without detail', () => {
    http.get('/api/test').subscribe({ error: () => {} });
    const req = httpMock.expectOne('/api/test');
    req.flush('Something went wrong', { status: 503, statusText: 'Service Unavailable' });
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      'Ocurrió un error inesperado.',
      'Cerrar',
      { duration: 5000, panelClass: ['snack-error'] }
    );
  });

  it('should rethrow the error after showing snackBar', (done) => {
    http.get('/api/test').subscribe({
      error: (err) => {
        expect(err.status).toBe(404);
        done();
      },
    });
    const req = httpMock.expectOne('/api/test');
    req.flush('Not Found', { status: 404, statusText: 'Not Found' });
  });

  it('should rethrow the 409 error without showing snackBar', (done) => {
    http.post('/api/test', {}).subscribe({
      error: (err) => {
        expect(err.status).toBe(409);
        done();
      },
    });
    const req = httpMock.expectOne('/api/test');
    req.flush('Conflict', { status: 409, statusText: 'Conflict' });
    expect(snackBarSpy.open).not.toHaveBeenCalled();
  });
});
