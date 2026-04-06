import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AgeRestrictionComponent } from './age-restriction.component';

describe('AgeRestrictionComponent', () => {
  let fixture: ComponentFixture<AgeRestrictionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgeRestrictionComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AgeRestrictionComponent);
    fixture.detectChanges();
  });

  it('renders the restriction banner', () => {
    const banner = fixture.nativeElement.querySelector('[data-testid="age-restriction-banner"]');
    expect(banner).toBeTruthy();
  });

  it('shows block message for 74+ insured', () => {
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('73 años');
  });

  it('mentions preexisting conditions form', () => {
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('Preexistencias');
  });
});
