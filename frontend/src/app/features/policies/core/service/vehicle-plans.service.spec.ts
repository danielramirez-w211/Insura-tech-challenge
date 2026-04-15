import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { VehiclePlansService } from './vehicle-plans.service';
import { VehiclePlan, VehicleQuotationResult } from '../models/vehicle-plan-selection.model';

const BASE_URL = 'http://localhost:5000/api/v1/vehicle-plans';

const mockPlan: VehiclePlan = {
  planId: 'standard',
  planName: 'Plan Estándar',
  priceMultiplier: 1.0,
  coverages: ['Robo', 'Pérdida total'],
  assistances: [],
};

const mockQuotation: VehicleQuotationResult = {
  commercialValue: 50_000_000,
  vehicleYear: 2026,
  brand: 'Toyota',
  vehicleAge: 0,
  ageCategory: 'Nuevo',
  technicalRate: 0.022,
  hasBrandSurcharge: false,
  baseMonthlyPremium: 91_667,
  plans: [
    { planId: 'standard', planName: 'Plan Estándar', monthlyPremium: 91_667,  annualPremiumWithDiscount: 1_045_000, coverages: ['Robo'], assistances: [] },
    { planId: 'complete', planName: 'Plan Completo', monthlyPremium: 110_000, annualPremiumWithDiscount: 1_254_000, coverages: ['Robo', 'Pérdida de llaves'], assistances: ['Grúa'] },
    { planId: 'premium',  planName: 'Plan Premium',  monthlyPremium: 132_917, annualPremiumWithDiscount: 1_515_250, coverages: ['Robo', 'Daño mecánico'],    assistances: ['Grúa', 'Carro de repuesto'] },
  ],
};

describe('VehiclePlansService', () => {
  let service: VehiclePlansService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        VehiclePlansService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service  = TestBed.inject(VehiclePlansService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ── getPlans() ─────────────────────────────────────────────────────────────

  describe('getPlans()', () => {
    it('should make GET request to /api/v1/vehicle-plans', () => {
      service.getPlans().subscribe();
      const req = httpMock.expectOne(BASE_URL);
      expect(req.request.method).toBe('GET');
      req.flush([mockPlan]);
    });

    it('should return list of VehiclePlan', (done) => {
      service.getPlans().subscribe((plans) => {
        expect(plans.length).toBe(1);
        expect(plans[0].planId).toBe('standard');
        done();
      });
      httpMock.expectOne(BASE_URL).flush([mockPlan]);
    });
  });

  // ── calculate() ───────────────────────────────────────────────────────────

  describe('calculate()', () => {
    it('should make GET request to /api/v1/vehicle-plans/calculate', () => {
      service.calculate(50_000_000, 2026, 'Toyota').subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/calculate`);
      expect(req.request.method).toBe('GET');
      req.flush(mockQuotation);
    });

    it('should send commercialValue as query param', () => {
      service.calculate(50_000_000, 2026, 'Toyota').subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/calculate`);
      expect(req.request.params.get('commercialValue')).toBe('50000000');
      req.flush(mockQuotation);
    });

    it('should send vehicleYear as query param', () => {
      service.calculate(50_000_000, 2023, 'Toyota').subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/calculate`);
      expect(req.request.params.get('vehicleYear')).toBe('2023');
      req.flush(mockQuotation);
    });

    it('should send brand as query param', () => {
      service.calculate(50_000_000, 2026, 'Renault').subscribe();
      const req = httpMock.expectOne((r) => r.url === `${BASE_URL}/calculate`);
      expect(req.request.params.get('brand')).toBe('Renault');
      req.flush(mockQuotation);
    });

    it('should return VehicleQuotationResult with three plans', (done) => {
      service.calculate(50_000_000, 2026, 'Toyota').subscribe((result) => {
        expect(result.brand).toBe('Toyota');
        expect(result.ageCategory).toBe('Nuevo');
        expect(result.plans.length).toBe(3);
        done();
      });
      httpMock.expectOne((r) => r.url === `${BASE_URL}/calculate`).flush(mockQuotation);
    });

    it('should reflect hasBrandSurcharge from response', (done) => {
      const surchargeQuotation = { ...mockQuotation, hasBrandSurcharge: true, brand: 'Renault' };
      service.calculate(30_000_000, 2023, 'Renault').subscribe((result) => {
        expect(result.hasBrandSurcharge).toBeTrue();
        done();
      });
      httpMock.expectOne((r) => r.url === `${BASE_URL}/calculate`).flush(surchargeQuotation);
    });
  });
});
