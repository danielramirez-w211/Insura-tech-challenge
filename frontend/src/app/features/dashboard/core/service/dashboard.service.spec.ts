import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DashboardService } from '../../services/dashboard.service';
import { PagedResult } from '../../../../core/models/api-response.model';

const API_URL = 'http://localhost:5000';

const mockEmptyPagedResult: PagedResult<unknown> = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 1,
};

function flushDashboardRequests(
  httpMock: HttpTestingController,
  overrides: { policies?: number; claims?: number; notifications?: number } = {}
) {
  const policiesReq = httpMock.expectOne(
    (r) => r.url === `${API_URL}/api/v1/policies` && r.params.get('status') === 'Active'
  );
  const claimsReq = httpMock.expectOne(
    (r) => r.url === `${API_URL}/api/v1/claims` && r.params.get('status') === 'Registered'
  );
  const notificationsReq = httpMock.expectOne(
    (r) => r.url === `${API_URL}/api/v1/notifications` && r.params.get('status') === 'Failed'
  );

  policiesReq.flush({ ...mockEmptyPagedResult, totalCount: overrides.policies ?? 0 });
  claimsReq.flush({ ...mockEmptyPagedResult, totalCount: overrides.claims ?? 0 });
  notificationsReq.flush({ ...mockEmptyPagedResult, totalCount: overrides.notifications ?? 0 });

  return { policiesReq, claimsReq, notificationsReq };
}

describe('DashboardService', () => {
  let service: DashboardService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(DashboardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial signal state', () => {
    it('should have metrics signal as null initially', () => {
      expect(service.metrics()).toBeNull();
    });

    it('should have loading signal as false initially', () => {
      expect(service.loading()).toBeFalse();
    });
  });

  describe('loadMetrics()', () => {
    it('should set loading to true when called', () => {
      service.loadMetrics().subscribe();
      expect(service.loading()).toBeTrue();
      flushDashboardRequests(httpMock);
    });

    it('should trigger a GET request to /api/v1/policies with Active status filter', () => {
      service.loadMetrics().subscribe();
      const req = httpMock.expectOne(
        (r) => r.url === `${API_URL}/api/v1/policies` && r.params.get('status') === 'Active'
      );
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('1');

      httpMock.expectOne((r) => r.url === `${API_URL}/api/v1/claims`).flush(mockEmptyPagedResult);
      httpMock.expectOne((r) => r.url === `${API_URL}/api/v1/notifications`).flush(mockEmptyPagedResult);
      req.flush(mockEmptyPagedResult);
    });

    it('should trigger a GET request to /api/v1/claims with Registered status filter', () => {
      service.loadMetrics().subscribe();
      const req = httpMock.expectOne(
        (r) => r.url === `${API_URL}/api/v1/claims` && r.params.get('status') === 'Registered'
      );
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('1');

      httpMock.expectOne((r) => r.url === `${API_URL}/api/v1/policies`).flush(mockEmptyPagedResult);
      httpMock.expectOne((r) => r.url === `${API_URL}/api/v1/notifications`).flush(mockEmptyPagedResult);
      req.flush(mockEmptyPagedResult);
    });

    it('should trigger a GET request to /api/v1/notifications with Failed status filter', () => {
      service.loadMetrics().subscribe();
      const req = httpMock.expectOne(
        (r) => r.url === `${API_URL}/api/v1/notifications` && r.params.get('status') === 'Failed'
      );
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('1');

      httpMock.expectOne((r) => r.url === `${API_URL}/api/v1/policies`).flush(mockEmptyPagedResult);
      httpMock.expectOne((r) => r.url === `${API_URL}/api/v1/claims`).flush(mockEmptyPagedResult);
      req.flush(mockEmptyPagedResult);
    });

    it('should trigger three parallel requests simultaneously (forkJoin)', () => {
      service.loadMetrics().subscribe();

      // All three requests should be pending simultaneously (forkJoin behavior)
      const allRequests = httpMock.match(() => true);
      expect(allRequests.length).toBe(3);
      allRequests.forEach((r) => r.flush(mockEmptyPagedResult));
    });

    it('should emit the combined result from forkJoin with correct totalCounts', (done) => {
      service.loadMetrics().subscribe((result) => {
        expect(result.policies.totalCount).toBe(42);
        expect(result.claims.totalCount).toBe(7);
        expect(result.notifications.totalCount).toBe(3);
        done();
      });

      flushDashboardRequests(httpMock, { policies: 42, claims: 7, notifications: 3 });
    });

    it('should complete the observable after all three requests resolve', (done) => {
      let completed = false;
      service.loadMetrics().subscribe({
        complete: () => {
          completed = true;
          expect(completed).toBeTrue();
          done();
        },
      });
      flushDashboardRequests(httpMock);
    });
  });
});
