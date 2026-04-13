import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { VehiclePlanSelectorComponent } from './vehicle-plan-selector.component';
import { VehiclePlanOption } from '../../../core/models/vehicle-plan-selection.model';

const MOCK_PLANS: VehiclePlanOption[] = [
  {
    planId: 'standard', planName: 'Plan Estándar',
    monthlyPremium: 91_667, annualPremiumWithDiscount: 1_045_000,
    coverages: ['Robo', 'Pérdida parcial'], assistances: [],
  },
  {
    planId: 'complete', planName: 'Plan Completo',
    monthlyPremium: 110_000, annualPremiumWithDiscount: 1_254_000,
    coverages: ['Robo', 'Pérdida parcial', 'Pérdida de llaves'], assistances: ['Grúa'],
  },
  {
    planId: 'premium', planName: 'Plan Premium',
    monthlyPremium: 132_917, annualPremiumWithDiscount: 1_515_250,
    coverages: ['Robo', 'Daño mecánico'], assistances: ['Grúa', 'Carro de repuesto'],
  },
];

describe('VehiclePlanSelectorComponent', () => {
  let fixture: ComponentFixture<VehiclePlanSelectorComponent>;
  let component: VehiclePlanSelectorComponent;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehiclePlanSelectorComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture   = TestBed.createComponent(VehiclePlanSelectorComponent);
    component = fixture.componentInstance;
    el        = fixture.nativeElement as HTMLElement;
    fixture.componentRef.setInput('plans', MOCK_PLANS);
    fixture.detectChanges();
  });

  // ── Render ─────────────────────────────────────────────────────────────────

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render three plan cards', () => {
    const cards = el.querySelectorAll('[data-testid^="vehicle-plan-"]');
    expect(cards.length).toBe(3);
  });

  it('should render standard plan card', () => {
    const card = el.querySelector('[data-testid="vehicle-plan-standard"]');
    expect(card).toBeTruthy();
  });

  it('should render complete plan card', () => {
    const card = el.querySelector('[data-testid="vehicle-plan-complete"]');
    expect(card).toBeTruthy();
  });

  it('should render premium plan card', () => {
    const card = el.querySelector('[data-testid="vehicle-plan-premium"]');
    expect(card).toBeTruthy();
  });

  it('should display plan names', () => {
    const text = el.textContent ?? '';
    expect(text).toContain('Plan Estándar');
    expect(text).toContain('Plan Completo');
    expect(text).toContain('Plan Premium');
  });

  it('should show assistances section only for plans that have them', () => {
    // Standard tiene assistances vacías — no debe mostrar sección
    const cards = el.querySelectorAll('[data-testid^="vehicle-plan-"]');
    // Standard card: no assistances
    const stdText = (cards[0] as HTMLElement).textContent ?? '';
    expect(stdText).not.toContain('Grúa');
    // Complete card: has Grúa
    const compText = (cards[1] as HTMLElement).textContent ?? '';
    expect(compText).toContain('Grúa');
  });

  // ── Selección ──────────────────────────────────────────────────────────────

  it('should emit planSelected when a card is clicked', () => {
    const emitted: VehiclePlanOption[] = [];
    component.planSelected.subscribe((p) => emitted.push(p));

    const card = el.querySelector<HTMLElement>('[data-testid="vehicle-plan-standard"]');
    card?.click();
    fixture.detectChanges();

    expect(emitted.length).toBe(1);
    expect(emitted[0].planId).toBe('standard');
  });

  it('should emit the correct plan object when complete is clicked', () => {
    const emitted: VehiclePlanOption[] = [];
    component.planSelected.subscribe((p) => emitted.push(p));

    const card = el.querySelector<HTMLElement>('[data-testid="vehicle-plan-complete"]');
    card?.click();
    fixture.detectChanges();

    expect(emitted[0].planId).toBe('complete');
    expect(emitted[0].monthlyPremium).toBe(110_000);
  });

  it('should apply "selected" class when selectedPlanId matches', () => {
    fixture.componentRef.setInput('selectedPlanId', 'complete');
    fixture.detectChanges();

    const card = el.querySelector('[data-testid="vehicle-plan-complete"]');
    expect(card?.classList.contains('selected')).toBeTrue();
  });

  it('should NOT apply "selected" class to unselected plans', () => {
    fixture.componentRef.setInput('selectedPlanId', 'complete');
    fixture.detectChanges();

    const stdCard  = el.querySelector('[data-testid="vehicle-plan-standard"]');
    const premCard = el.querySelector('[data-testid="vehicle-plan-premium"]');

    expect(stdCard?.classList.contains('selected')).toBeFalse();
    expect(premCard?.classList.contains('selected')).toBeFalse();
  });
});
