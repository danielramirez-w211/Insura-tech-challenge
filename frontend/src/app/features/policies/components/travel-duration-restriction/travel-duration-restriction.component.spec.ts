import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TravelDurationRestrictionComponent } from './travel-duration-restriction.component';

describe('TravelDurationRestrictionComponent', () => {
  let fixture: ComponentFixture<TravelDurationRestrictionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TravelDurationRestrictionComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(TravelDurationRestrictionComponent);
    fixture.detectChanges();
  });

  // FT-10
  it('should render the restriction banner', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[data-testid="duration-exceeded-banner"]')).toBeTruthy();
  });

  it('should mention 180 days in the message', () => {
    expect(fixture.nativeElement.textContent).toContain('180');
  });

  it('should mention renewing the policy', () => {
    expect(fixture.nativeElement.textContent).toContain('181');
  });
});
