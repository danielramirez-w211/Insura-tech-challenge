import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { VehicleDataFormComponent } from './vehicle-data-form.component';

describe('VehicleDataFormComponent', () => {
  let fixture: ComponentFixture<VehicleDataFormComponent>;
  let component: VehicleDataFormComponent;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleDataFormComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture   = TestBed.createComponent(VehicleDataFormComponent);
    component = fixture.componentInstance;
    el        = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  });

  // ── Render ─────────────────────────────────────────────────────────────────

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render commercialValue input', () => {
    const input = el.querySelector<HTMLInputElement>('[data-testid="vehicle-commercial-value"]');
    expect(input).toBeTruthy();
  });

  it('should render vehicleYear input', () => {
    const input = el.querySelector<HTMLInputElement>('[data-testid="vehicle-year"]');
    expect(input).toBeTruthy();
  });

  it('should render brand input', () => {
    // brand input has no explicit placeholder attr from spec but it's an input inside the form
    const input = el.querySelector<HTMLInputElement>('[data-testid="vehicle-brand"]');
    expect(input).toBeTruthy();
  });

  it('should render Cotizar button', () => {
    const btn = el.querySelector<HTMLButtonElement>('[data-testid="quote-button"]');
    expect(btn).toBeTruthy();
  });

  // ── Validación inicial ─────────────────────────────────────────────────────

  it('should start with an invalid form', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('should disable Cotizar button when form is invalid', () => {
    const btn = el.querySelector<HTMLButtonElement>('[data-testid="quote-button"]');
    expect(btn?.disabled).toBeTrue();
  });

  // ── Validaciones de campo ──────────────────────────────────────────────────

  it('commercialValue is required', () => {
    const ctrl = component.form.get('commercialValue');
    ctrl?.setValue(null);
    ctrl?.markAsTouched();
    expect(ctrl?.hasError('required')).toBeTrue();
  });

  it('commercialValue must be > 0', () => {
    const ctrl = component.form.get('commercialValue');
    ctrl?.setValue(0);
    ctrl?.markAsTouched();
    expect(ctrl?.hasError('min')).toBeTrue();
  });

  it('vehicleYear is required', () => {
    const ctrl = component.form.get('vehicleYear');
    ctrl?.setValue(null);
    ctrl?.markAsTouched();
    expect(ctrl?.hasError('required')).toBeTrue();
  });

  it('vehicleYear below 1900 is invalid', () => {
    const ctrl = component.form.get('vehicleYear');
    ctrl?.setValue(1899);
    ctrl?.markAsTouched();
    expect(ctrl?.hasError('vehicleYear')).toBeTrue();
  });

  it('vehicleYear above currentYear is invalid', () => {
    const ctrl = component.form.get('vehicleYear');
    ctrl?.setValue(new Date().getFullYear() + 1);
    ctrl?.markAsTouched();
    expect(ctrl?.hasError('vehicleYear')).toBeTrue();
  });

  it('vehicleYear equal to currentYear is valid', () => {
    const ctrl = component.form.get('vehicleYear');
    ctrl?.setValue(new Date().getFullYear());
    ctrl?.markAsTouched();
    expect(ctrl?.errors).toBeNull();
  });

  it('brand is required', () => {
    const ctrl = component.form.get('brand');
    ctrl?.setValue('');
    ctrl?.markAsTouched();
    expect(ctrl?.hasError('required')).toBeTrue();
  });

  // ── Emisión de evento ──────────────────────────────────────────────────────

  it('should emit quoteRequested with correct data when form is valid and button clicked', () => {
    // GIVEN — form válido
    component.form.setValue({ commercialValue: 50_000_000, vehicleYear: 2023, brand: 'Toyota' });
    fixture.detectChanges();

    const emitted: any[] = [];
    component.quoteRequested.subscribe((v: any) => emitted.push(v));

    // WHEN
    component.onQuote();

    // THEN
    expect(emitted.length).toBe(1);
    expect(emitted[0].commercialValue).toBe(50_000_000);
    expect(emitted[0].vehicleYear).toBe(2023);
    expect(emitted[0].brand).toBe('Toyota');
  });

  it('should NOT emit quoteRequested when form is invalid', () => {
    // GIVEN — form inválido (sin datos)
    const emitted: any[] = [];
    component.quoteRequested.subscribe((v: any) => emitted.push(v));

    // WHEN
    component.onQuote();

    // THEN
    expect(emitted.length).toBe(0);
  });

  it('should not emit when commercialValue is zero or negative', () => {
    component.form.setValue({ commercialValue: 0, vehicleYear: 2023, brand: 'Toyota' });
    fixture.detectChanges();

    const emitted: any[] = [];
    component.quoteRequested.subscribe((v: any) => emitted.push(v));

    component.onQuote();
    expect(emitted.length).toBe(0);
  });
});
