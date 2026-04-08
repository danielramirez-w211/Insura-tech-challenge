import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Subject, takeUntil, combineLatest } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { signal, computed } from '@angular/core';
import { PolicyType } from '../../../core/models/policy.model';
import { TripType, Continent } from '../../../core/models/travel-plan-selection.model';
import { HealthPlan, HealthPlanCalculation } from '../../../core/models/health-plan-selection.model';
import { TravelPlanSelection } from '../../../core/models/travel-plan-selection.model';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';
import { HealthPlanSelectorComponent } from '../../../components/health-plan-selector/health-plan-selector.component';
import { HealthPlanPreviewComponent } from '../../../components/health-plan-preview/health-plan-preview.component';
import { AgeRestrictionComponent } from '../../../components/age-restriction/age-restriction.component';
import { TravelPlanPreviewComponent } from '../../../components/travel-plan-preview/travel-plan-preview.component';
import { TravelDurationRestrictionComponent } from '../../../components/travel-duration-restriction/travel-duration-restriction.component';

export interface CoveragePeriodFormValue {
  type: PolicyType;
  startDate: string;
  endDate: string;
  insuredAmount: number | null;
  monthlyPremium: number | null;
  durationDays: number | null;
  tripType: TripType | null;
  continent: Continent | null;
  selectedPlanId: string | null;
  valid: boolean;
}

@Component({
  selector: 'app-coverage-period-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    StatusLabelPipe,
    HealthPlanSelectorComponent,
    HealthPlanPreviewComponent,
    AgeRestrictionComponent,
    TravelPlanPreviewComponent,
    TravelDurationRestrictionComponent,
  ],
  templateUrl: './coverage-period-form.component.html',
  styleUrl: './coverage-period-form.component.css',
})
export class CoveragePeriodFormComponent implements OnInit {
  @Input() healthPlans: HealthPlan[] = [];
  @Input() loadingPlans = false;
  @Input() healthCalculation: HealthPlanCalculation | null = null;
  @Input() travelCalculation: TravelPlanSelection | null = null;
  @Input() ageRestricted = false;

  @Output() formChange      = new EventEmitter<CoveragePeriodFormValue>();
  @Output() typeChange      = new EventEmitter<PolicyType>();
  @Output() planSelected    = new EventEmitter<HealthPlan>();
  @Output() tripTypeChange  = new EventEmitter<TripType>();
  @Output() continentChange = new EventEmitter<Continent>();
  @Output() next            = new EventEmitter<void>();
  @Output() back            = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();

  readonly typeOptions: PolicyType[] = ['Life', 'Health', 'Vehicle', 'Home', 'Travel'];

  isHealthType  = signal(false);
  isTravelType  = signal(false);
  tripType      = signal<TripType | null>(null);
  continent     = signal<Continent | null>(null);
  selectedPlanId = signal<string | null>(null);
  durationDays  = signal<number | null>(null);

  durationExceeded = computed(() => {
    const d = this.durationDays();
    return d !== null && d > 180;
  });

  form = this.fb.group({
    type:           ['Life' as PolicyType, Validators.required],
    insuredAmount:  [null as number | null, [Validators.min(1)]],
    monthlyPremium: [null as number | null, [Validators.required, Validators.min(1)]],
    startDate:      [null as Date | null, Validators.required],
    endDate:        [null as Date | null, Validators.required],
  });

  get isNextDisabled(): boolean {
    if (this.form.invalid) return true;
    if (this.isHealthType() && (this.ageRestricted || !this.selectedPlanId() || !this.healthCalculation)) return true;
    if (this.isTravelType() && (!this.tripType() || !this.durationDays() || this.durationExceeded() || !this.travelCalculation)) return true;
    if (this.isTravelType() && this.tripType() === 'Internacional' && !this.continent()) return true;
    return false;
  }

  ngOnInit(): void {
    combineLatest([
      this.form.get('startDate')!.valueChanges,
      this.form.get('endDate')!.valueChanges,
    ]).pipe(takeUntil(this.destroy$))
      .subscribe(([start, end]) => {
        if (start && end) {
          const days = Math.round((end.getTime() - start.getTime()) / 86_400_000) + 1;
          this.durationDays.set(days > 0 ? days : null);
        } else {
          this.durationDays.set(null);
        }
        this.emitChange();
      });

    this.form.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => this.emitChange());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onTypeChange(type: PolicyType): void {
    this.isHealthType.set(type === 'Health');
    this.isTravelType.set(type === 'Travel');
    this.selectedPlanId.set(null);
    this.tripType.set(null);
    this.continent.set(null);
    this.durationDays.set(null);
    this.typeChange.emit(type);
    this.emitChange();
  }

  onTripTypeChange(type: TripType): void {
    this.tripType.set(type);
    this.continent.set(null);
    this.tripTypeChange.emit(type);
    this.emitChange();
  }

  onContinentChange(cont: Continent): void {
    this.continent.set(cont);
    this.continentChange.emit(cont);
    this.emitChange();
  }

  onPlanSelected(plan: HealthPlan): void {
    this.selectedPlanId.set(plan.planId);
    this.planSelected.emit(plan);
    this.emitChange();
  }

  onNext(): void {
    if (!this.isNextDisabled) this.next.emit();
  }

  private toDateStr(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  private emitChange(): void {
    const v = this.form.value;
    const start = v.startDate ? this.toDateStr(v.startDate) : '';
    const end   = v.endDate   ? this.toDateStr(v.endDate)   : '';

    this.formChange.emit({
      type:          v.type ?? 'Life',
      startDate:     start,
      endDate:       end,
      insuredAmount: v.insuredAmount ?? null,
      monthlyPremium: v.monthlyPremium ?? null,
      durationDays:  this.durationDays(),
      tripType:      this.tripType(),
      continent:     this.continent(),
      selectedPlanId: this.selectedPlanId(),
      valid:         !this.isNextDisabled,
    });
  }
}
