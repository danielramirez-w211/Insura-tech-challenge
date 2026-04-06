import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HealthPlanPreviewComponent } from './health-plan-preview.component';
import { HealthPlanCalculationDto } from '../../models/policy.model';

const MOCK_CALC: HealthPlanCalculationDto = {
  planId: 'salud-premium',
  planName: 'Salud Premium',
  baseAmount: 450_000,
  ageFactorPercentage: 4,
  ageFactorAmount: 18_000,
  finalAmount: 468_000,
  insuredAge: 40,
};

describe('HealthPlanPreviewComponent', () => {
  let fixture: ComponentFixture<HealthPlanPreviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthPlanPreviewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HealthPlanPreviewComponent);
  });

  it('renders nothing when calculation is null', () => {
    fixture.componentRef.setInput('calculation', null);
    fixture.detectChanges();

    const preview = fixture.nativeElement.querySelector('[data-testid="health-plan-preview"]');
    expect(preview).toBeNull();
  });

  it('renders preview card when calculation is provided', () => {
    fixture.componentRef.setInput('calculation', MOCK_CALC);
    fixture.detectChanges();

    const preview = fixture.nativeElement.querySelector('[data-testid="health-plan-preview"]');
    expect(preview).toBeTruthy();
  });

  it('displays plan name', () => {
    fixture.componentRef.setInput('calculation', MOCK_CALC);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Salud Premium');
  });

  it('displays age factor percentage', () => {
    fixture.componentRef.setInput('calculation', MOCK_CALC);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('4%');
  });

  it('displays final amount in the final-amount element', () => {
    fixture.componentRef.setInput('calculation', MOCK_CALC);
    fixture.detectChanges();

    const el = fixture.nativeElement.querySelector('[data-testid="final-amount"]');
    expect(el).toBeTruthy();
    expect(el.textContent).toContain('468');
  });

  it('displays insured age', () => {
    fixture.componentRef.setInput('calculation', MOCK_CALC);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('40');
  });
});
