import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { idempotencyInterceptor } from './idempotency.interceptor';

describe('idempotencyInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([idempotencyInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should add Idempotency-Key header to POST requests', () => {
    http.post('/api/test', {}).subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Idempotency-Key')).toBeTrue();
    req.flush({});
  });

  it('should NOT add Idempotency-Key header to GET requests', () => {
    http.get('/api/test').subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Idempotency-Key')).toBeFalse();
    req.flush([]);
  });

  it('should NOT add Idempotency-Key header to PUT requests', () => {
    http.put('/api/test', {}).subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Idempotency-Key')).toBeFalse();
    req.flush({});
  });

  it('should NOT add Idempotency-Key header to DELETE requests', () => {
    http.delete('/api/test').subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Idempotency-Key')).toBeFalse();
    req.flush({});
  });

  it('should add a non-empty UUID as Idempotency-Key on POST', () => {
    http.post('/api/test', {}).subscribe();
    const req = httpMock.expectOne('/api/test');
    const key = req.request.headers.get('Idempotency-Key');
    expect(key).toBeTruthy();
    // UUID v4 format: xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx
    const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    expect(uuidRegex.test(key!)).toBeTrue();
    req.flush({});
  });

  it('should generate a unique Idempotency-Key for each POST request', () => {
    http.post('/api/test', {}).subscribe();
    http.post('/api/test2', {}).subscribe();

    const req1 = httpMock.expectOne('/api/test');
    const req2 = httpMock.expectOne('/api/test2');

    const key1 = req1.request.headers.get('Idempotency-Key');
    const key2 = req2.request.headers.get('Idempotency-Key');

    expect(key1).not.toBe(key2);

    req1.flush({});
    req2.flush({});
  });

  it('should pass the request body unchanged on POST', () => {
    const body = { name: 'test', value: 42 };
    http.post('/api/test', body).subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });
});
