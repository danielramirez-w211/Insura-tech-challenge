import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TravelPlansService } from './travel-plans.service';
import { environment } from '../../../../environments/environment';

describe('TravelPlansService', () => {
  let service: TravelPlansService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TravelPlansService],
    });
    service = TestBed.inject(TravelPlansService);
    http    = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  // FT-09 variant
  it('should call calculate endpoint for Nacional', () => {
    service.calculate('Nacional', 10).subscribe();

    const req = http.expectOne(r =>
      r.url.includes('/travel-plans/calculate') &&
      r.params.get('tripType') === 'Nacional' &&
      r.params.get('durationDays') === '10'
    );
    expect(req.request.method).toBe('GET');
    req.flush({ tripType: 'Nacional', totalPriceCop: 13000, durationDays: 10 });
  });

  it('should include continent param for Internacional', () => {
    service.calculate('Internacional', 10, 'Europe').subscribe();

    const req = http.expectOne(r =>
      r.url.includes('/travel-plans/calculate') &&
      r.params.get('continent') === 'Europe'
    );
    expect(req.request.method).toBe('GET');
    req.flush({ tripType: 'Internacional', totalPriceCop: 315000, durationDays: 10 });
  });

  it('should not include continent param for Nacional', () => {
    service.calculate('Nacional', 5).subscribe();

    const req = http.expectOne(r => r.url.includes('/travel-plans/calculate'));
    expect(req.request.params.has('continent')).toBeFalse();
    req.flush({ tripType: 'Nacional', totalPriceCop: 6800, durationDays: 5 });
  });
});
