import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { VehiclePlanPreviewComponent } from './vehicle-plan-preview.component';
import { VehiclePlanOption, VehicleQuotationResult } from '../../../core/models/vehicle-plan-selection.model';

const MOCK_QUOTATION: VehicleQuotationResult = {
  commercialValue: 50_000_000,
  vehicleYear: 2023,
  brand: 'Toyota',
  vehicleAge: 3,
  ageCategory: 'Usado Reciente',
  technicalRate: 0.028,
  hasBrandSurcharge: false,
  baseMonthlyPremium: 116_667,
  plans: [],
};

const MOCK_STANDARD_PLAN: VehiclePlanOption = {
  planId: 'standard',
  planName: 'Plan Estándar',
  monthlyPremium: 116_667,
  annualPremiumWithDiscount: 1_330_000,
  coverages: ['Robo', 'Pérdida parcial', 'Pérdida total', 'Daños a terceros', 'Rayones a carrocería'],
  assistances: [],
};

const MOCK_PREMIUM_PLAN: VehiclePlanOption = {
  planId: 'premium',
  planName: 'Plan Premium',
  monthlyPremium: 169_167,
  annualPremiumWithDiscount: 1_928_650,
  coverages: ['Robo', 'Pérdida parcial', 'Daño mecánico'],
  assistances: ['Grúa', 'Carro de repuesto', 'Mecánico a casa', 'Conductor elegido'],
};

describe('VehiclePlanPreviewComponent', () => {
  let fixture: ComponentFixture<VehiclePlanPreviewComponent>;
  let component: VehiclePlanPreviewComponent;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehiclePlanPreviewComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture   = TestBed.createComponent(VehiclePlanPreviewComponent);
    component = fixture.componentInstance;
    el        = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  // ── Sin datos — no renderiza ───────────────────────────────────────────────

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should NOT render preview when quotation and plan are null', () => {
    const preview = el.querySelector('[data-testid="vehicle-plan-preview"]');
    expect(preview).toBeNull();
  });

  it('should NOT render preview when only quotation is set', () => {
    fixture.componentRef.setInput('quotation', MOCK_QUOTATION);
    fixture.detectChanges();
    const preview = el.querySelector('[data-testid="vehicle-plan-preview"]');
    expect(preview).toBeNull();
  });

  // ── Con datos — renderiza ──────────────────────────────────────────────────

  describe('with quotation and selectedPlan set', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('quotation', MOCK_QUOTATION);
      fixture.componentRef.setInput('selectedPlan', MOCK_STANDARD_PLAN);
      fixture.detectChanges();
    });

    it('should render the preview card', () => {
      const preview = el.querySelector('[data-testid="vehicle-plan-preview"]');
      expect(preview).toBeTruthy();
    });

    it('should display monthly premium correctly', () => {
      const monthlyEl = el.querySelector('[data-testid="vehicle-monthly-premium"]');
      expect(monthlyEl?.textContent).toContain('116');
    });

    it('should display annual premium with discount', () => {
      const annualEl = el.querySelector('[data-testid="vehicle-annual-premium"]');
      expect(annualEl?.textContent).toContain('1,330');
    });

    it('should display brand', () => {
      expect(el.textContent).toContain('Toyota');
    });

    it('should display vehicle year', () => {
      expect(el.textContent).toContain('2023');
    });

    it('should display age category', () => {
      expect(el.textContent).toContain('Usado Reciente');
    });

    it('should display plan name', () => {
      expect(el.textContent).toContain('Plan Estándar');
    });

    it('should list coverages', () => {
      expect(el.textContent).toContain('Robo');
      expect(el.textContent).toContain('Pérdida parcial');
    });

    it('should NOT show assistances section when plan has none (Standard)', () => {
      // No debe mostrar "Grúa" ni encabezado de asistencias
      expect(el.textContent).not.toContain('Grúa');
    });

    it('should NOT show brand surcharge warning when hasBrandSurcharge is false', () => {
      expect(el.textContent).not.toContain('alta siniestralidad');
    });
  });

  // ── Plan Premium — asistencias ─────────────────────────────────────────────

  describe('with Premium plan', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('quotation', MOCK_QUOTATION);
      fixture.componentRef.setInput('selectedPlan', MOCK_PREMIUM_PLAN);
      fixture.detectChanges();
    });

    it('should show assistances only for Premium plan', () => {
      expect(el.textContent).toContain('Grúa');
      expect(el.textContent).toContain('Carro de repuesto');
      expect(el.textContent).toContain('Mecánico a casa');
      expect(el.textContent).toContain('Conductor elegido');
    });

    it('should display correct monthly premium for Premium plan', () => {
      const monthlyEl = el.querySelector('[data-testid="vehicle-monthly-premium"]');
      expect(monthlyEl?.textContent).toContain('169');
    });
  });

  // ── Recargo por marca ─────────────────────────────────────────────────────

  it('should show brand surcharge warning when hasBrandSurcharge is true', () => {
    const surchargeQuotation: VehicleQuotationResult = { ...MOCK_QUOTATION, hasBrandSurcharge: true, brand: 'Renault' };
    fixture.componentRef.setInput('quotation', surchargeQuotation);
    fixture.componentRef.setInput('selectedPlan', MOCK_STANDARD_PLAN);
    fixture.detectChanges();

    expect(el.textContent).toContain('alta siniestralidad');
  });
});
