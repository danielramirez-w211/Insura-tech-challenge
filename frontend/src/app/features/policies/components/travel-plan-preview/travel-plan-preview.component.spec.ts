import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TravelPlanPreviewComponent } from './travel-plan-preview.component';
import { TravelPlanCalculationDto } from '../../models/policy.model';

const NACIONAL_CALC: TravelPlanCalculationDto = {
  tripType: 'Nacional',
  durationDays: 10,
  basePriceCop: 2200,
  dailyIncrementCop: 1200,
  totalPriceCop: 13000,
  calculatedAt: '2026-04-07T00:00:00Z',
};

const INTL_CALC: TravelPlanCalculationDto = {
  tripType: 'Internacional',
  continent: 'Europe',
  durationDays: 10,
  basePriceUsd: 30,
  basePriceCop: 126000,
  dailyIncrementCop: 21000,
  totalPriceCop: 315000,
  trmUsed: 4200,
  trmDate: '2026-04-07',
  calculatedAt: '2026-04-07T00:00:00Z',
};

describe('TravelPlanPreviewComponent', () => {
  let fixture: ComponentFixture<TravelPlanPreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TravelPlanPreviewComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(TravelPlanPreviewComponent);
  });

  // FT-05
  it('should not render anything when calculation is null', () => {
    fixture.componentRef.setInput('calculation', null);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="travel-plan-preview"]')).toBeNull();
  });

  // FT-06
  it('should render Nacional calculation with final amount', () => {
    fixture.componentRef.setInput('calculation', NACIONAL_CALC);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="travel-plan-preview"]')).toBeTruthy();
    const amount = el.querySelector('[data-testid="final-amount-cop"]');
    expect(amount?.textContent).toContain('13');
  });

  // FT-07
  it('should show TRM value for Internacional calculation', () => {
    fixture.componentRef.setInput('calculation', INTL_CALC);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="trm-value"]')).toBeTruthy();
  });

  // FT-08
  it('should show all key fields for Nacional', () => {
    fixture.componentRef.setInput('calculation', NACIONAL_CALC);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('Nacional');
    expect(text).toContain('10');
  });

  it('should show continent for Internacional', () => {
    fixture.componentRef.setInput('calculation', INTL_CALC);
    fixture.detectChanges();
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('Europe');
  });

  it('should not show TRM for Nacional', () => {
    fixture.componentRef.setInput('calculation', NACIONAL_CALC);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="trm-value"]')).toBeNull();
  });
});
