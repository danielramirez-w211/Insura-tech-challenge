import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PoliciesService } from './policies.service';
import { PolicyDto, CreatePolicyRequest } from '../models/policy.model';
import { PagedResult } from '../../../core/models/api-response.model';

const BASE_URL = 'http://localhost:5000/api/v1/policies';

const mockPolicy: PolicyDto = {
  id: 'policy-1',
  policyNumber: 'POL-001',
  status: 'Active',
  type: 'Life',
  insured: {
    name: 'John Doe',
    documentId: 'DOC-001',
    birthDate: '1990-01-01',
    email: 'john@example.com',
    phone: '555-0100',
  },
  coveragePeriod: {
    startDate: '2025-01-01',
    endDate: '2026-01-01',
  },
  insuredAmount: 100000,
  createdAt: '2025-01-01T00:00:00Z',
};

const mockPagedResult: PagedResult<PolicyDto> = {
  items: [mockPolicy],
  totalCount: 1,
  page: 1,
  pageSize: 10,
};

const mockCreateRequest: CreatePolicyRequest = {
  type: 'Life',
  insured: {
    name: 'John Doe',
    documentId: 'DOC-001',
    birthDate: '1990-01-01',
    email: 'john@example.com',
    phone: '555-0100',
  },
  coveragePeriod: {
    startDate: '2025-01-01',
    endDate: '2026-01-01',
  },
  insuredAmount: 100000,
  monthlyPremium: 500,
};

describe('PoliciesService', () => {
  let service: PoliciesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        PoliciesService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(PoliciesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial signal state', () => {
    it('should have empty policies signal initially', () => {
      expect(service.policies()).toEqual([]);
    });

    it('should have loading signal as false initially', () => {
      expect(service.loading()).toBeFalse();
    });

    it('should have totalCount signal as 0 initially', () => {
      expect(service.totalCount()).toBe(0);
    });
  });

  describe('getPolicies()', () => {
    it('should make GET request to /api/v1/policies', () => {
      service.getPolicies({ page: 1, pageSize: 10 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResult);
    });

    it('should include page and pageSize as query params', () => {
      service.getPolicies({ page: 2, pageSize: 5 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('page')).toBe('2');
      expect(req.request.params.get('pageSize')).toBe('5');
      req.flush(mockPagedResult);
    });

    it('should include optional status filter when provided', () => {
      service.getPolicies({ page: 1, pageSize: 10, status: 'Active' }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('status')).toBe('Active');
      req.flush(mockPagedResult);
    });

    it('should not include undefined filters in query params', () => {
      service.getPolicies({ page: 1, pageSize: 10, status: undefined }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.has('status')).toBeFalse();
      req.flush(mockPagedResult);
    });

    it('should return the paged result observable', (done) => {
      service.getPolicies({ page: 1, pageSize: 10 }).subscribe((result) => {
        expect(result.items.length).toBe(1);
        expect(result.totalCount).toBe(1);
        done();
      });
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      req.flush(mockPagedResult);
    });

    it('should return empty result when no policies exist', (done) => {
      const emptyResult: PagedResult<PolicyDto> = { items: [], totalCount: 0, page: 1, pageSize: 10 };
      service.getPolicies({ page: 1, pageSize: 10 }).subscribe((result) => {
        expect(result.totalCount).toBe(0);
        expect(result.items).toEqual([]);
        done();
      });
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      req.flush(emptyResult);
    });
  });

  describe('getPolicy()', () => {
    it('should make GET request to /api/v1/policies/{id}', () => {
      service.getPolicy('policy-1').subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/policy-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPolicy);
    });

    it('should return the policy DTO', (done) => {
      service.getPolicy('policy-1').subscribe((policy) => {
        expect(policy.id).toBe('policy-1');
        expect(policy.policyNumber).toBe('POL-001');
        expect(policy.type).toBe('Life');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/policy-1`);
      req.flush(mockPolicy);
    });
  });

  describe('createPolicy()', () => {
    it('should make POST request to /api/v1/policies', () => {
      service.createPolicy(mockCreateRequest).subscribe();
      const req = httpMock.expectOne(BASE_URL);
      expect(req.request.method).toBe('POST');
      req.flush(mockPolicy);
    });

    it('should send the request body to the policies endpoint', () => {
      service.createPolicy(mockCreateRequest).subscribe();
      const req = httpMock.expectOne(BASE_URL);
      expect(req.request.body).toEqual(mockCreateRequest);
      req.flush(mockPolicy);
    });

    it('should return the created policy', (done) => {
      service.createPolicy(mockCreateRequest).subscribe((policy) => {
        expect(policy.id).toBe('policy-1');
        expect(policy.status).toBe('Active');
        done();
      });
      const req = httpMock.expectOne(BASE_URL);
      req.flush(mockPolicy);
    });
  });

  describe('activatePolicy()', () => {
    it('should make PUT request to /api/v1/policies/{id}/activate', () => {
      service.activatePolicy('policy-1').subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/policy-1/activate`);
      expect(req.request.method).toBe('PUT');
      req.flush({ ...mockPolicy, status: 'Active' });
    });

    it('should send an empty body to the activate endpoint', () => {
      service.activatePolicy('policy-1').subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/policy-1/activate`);
      expect(req.request.body).toEqual({});
      req.flush({ ...mockPolicy, status: 'Active' });
    });

    it('should return the updated policy', (done) => {
      service.activatePolicy('policy-1').subscribe((policy) => {
        expect(policy.status).toBe('Active');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/policy-1/activate`);
      req.flush({ ...mockPolicy, status: 'Active' });
    });
  });

  describe('getClaimsByPolicy()', () => {
    it('should make GET request to /api/v1/policies/{id}/claims', () => {
      service.getClaimsByPolicy('policy-1', { page: 1, pageSize: 10 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/policy-1/claims`);
      expect(req.request.method).toBe('GET');
      req.flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });
    });

    it('should include pagination params for claims request', () => {
      service.getClaimsByPolicy('policy-1', { page: 1, pageSize: 5 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/policy-1/claims`);
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('5');
      req.flush({ items: [], totalCount: 0, page: 1, pageSize: 5 });
    });

    it('should include status filter in claims request when provided', () => {
      service.getClaimsByPolicy('policy-1', { page: 1, pageSize: 10, status: 'Registered' }).subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/policy-1/claims`);
      expect(req.request.params.get('status')).toBe('Registered');
      req.flush({ items: [], totalCount: 0, page: 1, pageSize: 10 });
    });
  });
});
