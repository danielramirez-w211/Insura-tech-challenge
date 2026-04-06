import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HealthPlanSelectorComponent } from './health-plan-selector.component';
import { HealthPlanDto } from '../../models/policy.model';

const MOCK_PLANS: HealthPlanDto[] = [
  { planId: 'basic', planName: 'Básico', baseAmount: 300_000 },
  { planId: 'salud-global', planName: 'Salud Global', baseAmount: 380_000 },
  { planId: 'salud-premium', planName: 'Salud Premium', baseAmount: 450_000 },
  { planId: 'salud-vida-total', planName: 'Salud Vida Total', baseAmount: 600_000 },
];

describe('HealthPlanSelectorComponent', () => {
  let fixture: ComponentFixture<HealthPlanSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthPlanSelectorComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HealthPlanSelectorComponent);
    fixture.componentRef.setInput('plans', MOCK_PLANS);
    fixture.componentRef.setInput('selectedPlanId', null);
    fixture.detectChanges();
  });

  it('renders all 4 plans', () => {
    const cards = fixture.nativeElement.querySelectorAll('.plan-card');
    expect(cards.length).toBe(4);
  });

  it('displays plan names', () => {
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('Básico');
    expect(text).toContain('Salud Global');
    expect(text).toContain('Salud Premium');
    expect(text).toContain('Salud Vida Total');
  });

  it('emits planSelected when a plan card is clicked', () => {
    let emitted: HealthPlanDto | undefined;
    fixture.componentInstance.planSelected.subscribe((p: HealthPlanDto) => (emitted = p));

    const card = fixture.nativeElement.querySelector('[data-testid="plan-basic"]');
    card.click();

    expect(emitted).toBeDefined();
    expect(emitted!.planId).toBe('basic');
  });

  it('highlights selected plan with "selected" class', () => {
    fixture.componentRef.setInput('selectedPlanId', 'salud-premium');
    fixture.detectChanges();

    const selected = fixture.nativeElement.querySelector('.plan-card.selected');
    expect(selected).toBeTruthy();
    expect(selected.getAttribute('data-testid')).toBe('plan-salud-premium');
  });

  it('does not highlight unselected plans', () => {
    fixture.componentRef.setInput('selectedPlanId', 'basic');
    fixture.detectChanges();

    const allCards = fixture.nativeElement.querySelectorAll('.plan-card');
    const selectedCards = fixture.nativeElement.querySelectorAll('.plan-card.selected');
    expect(allCards.length).toBe(4);
    expect(selectedCards.length).toBe(1);
  });
});
