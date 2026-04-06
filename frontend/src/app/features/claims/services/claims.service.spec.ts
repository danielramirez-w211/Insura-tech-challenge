import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ClaimsService } from './claims.service';
import { ClaimDto, ClaimActionRequest, CreateClaimRequest } from '../models/claim.model';
import { PagedResult } from '../../../core/models/api-response.model';

const BASE_URL = 'http://localhost:5000/api/v1/claims';

const mockClaim: ClaimDto = {
  id: 'claim-1',
  claimNumber: 'CLM-001',
  policyId: 'policy-1',
  policyNumber: 'POL-001',
  description: 'Test claim description',
  claimAmount: 5000,
  status: 'Registered',
  statusHistory: [
    {
      status: 'Registered',
      changedAt: '2025-01-01T00:00:00Z',
      responsibleUser: 'system',
    },
  ],
  createdAt: '2025-01-01T00:00:00Z',
};

const mockPagedResult: PagedResult<ClaimDto> = {
  items: [mockClaim],
  totalCount: 1,
  page: 1,
  pageSize: 10,
};

describe('ClaimsService', () => {
  let service: ClaimsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ClaimsService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(ClaimsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial signal state', () => {
    it('should have empty claims signal initially', () => {
      expect(service.claims()).toEqual([]);
    });

    it('should have loading signal as false initially', () => {
      expect(service.loading()).toBeFalse();
    });

    it('should have totalCount signal as 0 initially', () => {
      expect(service.totalCount()).toBe(0);
    });

    it('should have selectedClaim signal as null initially', () => {
      expect(service.selectedClaim()).toBeNull();
    });
  });

  describe('getClaims()', () => {
    it('should make GET request to /api/v1/claims', () => {
      service.getClaims({ page: 1, pageSize: 10 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResult);
    });

    it('should include page and pageSize as query params', () => {
      service.getClaims({ page: 2, pageSize: 5 }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('page')).toBe('2');
      expect(req.request.params.get('pageSize')).toBe('5');
      req.flush(mockPagedResult);
    });

    it('should include optional status filter when provided', () => {
      service.getClaims({ page: 1, pageSize: 10, status: 'Registered' }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('status')).toBe('Registered');
      req.flush(mockPagedResult);
    });

    it('should include optional policyId filter when provided', () => {
      service.getClaims({ page: 1, pageSize: 10, policyId: 'policy-1' }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.get('policyId')).toBe('policy-1');
      req.flush(mockPagedResult);
    });

    it('should not include undefined filters in query params', () => {
      service.getClaims({ page: 1, pageSize: 10, status: undefined, policyId: undefined }).subscribe();
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      expect(req.request.params.has('status')).toBeFalse();
      expect(req.request.params.has('policyId')).toBeFalse();
      req.flush(mockPagedResult);
    });

    it('should return the paged result observable', (done) => {
      service.getClaims({ page: 1, pageSize: 10 }).subscribe((result) => {
        expect(result.items.length).toBe(1);
        expect(result.totalCount).toBe(1);
        done();
      });
      const req = httpMock.expectOne((r) => r.url === BASE_URL);
      req.flush(mockPagedResult);
    });
  });

  describe('getClaim()', () => {
    it('should make GET request to /api/v1/claims/{id}', () => {
      service.getClaim('claim-1').subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockClaim);
    });

    it('should return the claim DTO', (done) => {
      service.getClaim('claim-1').subscribe((claim) => {
        expect(claim.id).toBe('claim-1');
        expect(claim.claimNumber).toBe('CLM-001');
        expect(claim.status).toBe('Registered');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/claim-1`);
      req.flush(mockClaim);
    });
  });

  describe('createClaim()', () => {
    it('should make POST request to /api/v1/claims', () => {
      const request: CreateClaimRequest = {
        policyId: 'policy-1',
        description: 'Test claim',
        claimAmount: 5000,
      };
      service.createClaim(request).subscribe();
      const req = httpMock.expectOne(BASE_URL);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockClaim);
    });
  });

  describe('approveClaim()', () => {
    it('should make PUT request to /api/v1/claims/{id}/approve', () => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'adjuster-1', observations: 'Approved' };
      service.approveClaim('claim-1', actionRequest).subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/approve`);
      expect(req.request.method).toBe('PUT');
      req.flush({ ...mockClaim, status: 'Approved' });
    });

    it('should send the action request body to the approve endpoint', () => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'adjuster-1', observations: 'All good' };
      service.approveClaim('claim-1', actionRequest).subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/approve`);
      expect(req.request.body).toEqual(actionRequest);
      req.flush({ ...mockClaim, status: 'Approved' });
    });

    it('should return the updated claim with Approved status', (done) => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'adjuster-1' };
      service.approveClaim('claim-1', actionRequest).subscribe((claim) => {
        expect(claim.status).toBe('Approved');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/approve`);
      req.flush({ ...mockClaim, status: 'Approved' });
    });
  });

  describe('rejectClaim()', () => {
    it('should make PUT request to /api/v1/claims/{id}/reject', () => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'adjuster-1', observations: 'Rejected' };
      service.rejectClaim('claim-1', actionRequest).subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/reject`);
      expect(req.request.method).toBe('PUT');
      req.flush({ ...mockClaim, status: 'Rejected' });
    });

    it('should send the action request body to the reject endpoint', () => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'adjuster-1', observations: 'Insufficient evidence' };
      service.rejectClaim('claim-1', actionRequest).subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/reject`);
      expect(req.request.body).toEqual(actionRequest);
      req.flush({ ...mockClaim, status: 'Rejected' });
    });

    it('should return the updated claim with Rejected status', (done) => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'adjuster-1' };
      service.rejectClaim('claim-1', actionRequest).subscribe((claim) => {
        expect(claim.status).toBe('Rejected');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/reject`);
      req.flush({ ...mockClaim, status: 'Rejected' });
    });
  });

  describe('appealClaim()', () => {
    it('should make PUT request to /api/v1/claims/{id}/appeal', () => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'customer-1', observations: 'I disagree' };
      service.appealClaim('claim-1', actionRequest).subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/appeal`);
      expect(req.request.method).toBe('PUT');
      req.flush({ ...mockClaim, status: 'Appealed' });
    });

    it('should send the action request body to the appeal endpoint', () => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'customer-1', observations: 'New evidence' };
      service.appealClaim('claim-1', actionRequest).subscribe();
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/appeal`);
      expect(req.request.body).toEqual(actionRequest);
      req.flush({ ...mockClaim, status: 'Appealed' });
    });

    it('should return the updated claim with Appealed status', (done) => {
      const actionRequest: ClaimActionRequest = { responsibleUser: 'customer-1' };
      service.appealClaim('claim-1', actionRequest).subscribe((claim) => {
        expect(claim.status).toBe('Appealed');
        done();
      });
      const req = httpMock.expectOne(`${BASE_URL}/claim-1/appeal`);
      req.flush({ ...mockClaim, status: 'Appealed' });
    });
  });
});
