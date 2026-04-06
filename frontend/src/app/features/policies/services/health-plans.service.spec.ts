import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { HealthPlansService } from './health-plans.service';
import { environment } from '../../../../environments/environment';
import { HealthPlanCalculationDto, HealthPlanDto } from '../models/policy.model';

describe('HealthPlansService', () => {
  let service: HealthPlansService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiUrl}/api/v1/health-plans`;

  const mockPlans: HealthPlanDto[] = [
    { planId: 'basic', planName: 'Básico', baseAmount: 300_000 },
    { planId: 'salud-global', planName: 'Salud Global', baseAmount: 380_000 },
    { planId: 'salud-premium', planName: 'Salud Premium', baseAmount: 450_000 },
    { planId: 'salud-vida-total', planName: 'Salud Vida Total', baseAmount: 600_000 },
  ];

  const mockCalc: HealthPlanCalculationDto = {
    planId: 'salud-premium',
    planName: 'Salud Premium',
    baseAmount: 450_000,
    ageFactorPercentage: 4,
    ageFactorAmount: 18_000,
    finalAmount: 468_000,
    insuredAge: 40,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(HealthPlansService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getPlans - calls GET /api/v1/health-plans', () => {
    service.getPlans().subscribe(plans => {
      expect(plans).toHaveSize(4);
      expect(plans[0].planId).toBe('basic');
    });

    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('GET');
    req.flush(mockPlans);
  });

  it('calculate - calls correct endpoint with planId and birthDate params', () => {
    service.calculate('salud-premium', '1984-06-15').subscribe(calc => {
      expect(calc.finalAmount).toBe(468_000);
    });

    const req = httpMock.expectOne(
      r => r.url === `${base}/calculate`
        && r.params.get('planId') === 'salud-premium'
        && r.params.get('birthDate') === '1984-06-15'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockCalc);
  });

  it('calculate - passes planId as query param', () => {
    service.calculate('basic', '2000-01-01').subscribe();

    const req = httpMock.expectOne(r => r.url === `${base}/calculate`);
    expect(req.request.params.get('planId')).toBe('basic');
    req.flush(mockCalc);
  });
});
