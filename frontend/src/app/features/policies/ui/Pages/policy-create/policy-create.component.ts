import { Component, inject, signal, computed, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Subject, takeUntil, catchError, EMPTY, combineLatest } from 'rxjs';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PoliciesCoreService } from '../../../core/service/policies.service';
import { HealthPlansService } from '../../../core/service/health-plans.service';
import { TravelPlansService } from '../../../core/service/travel-plans.service';
import { PolicyType } from '../../../core/models/policy.model';
import { HealthPlan, HealthPlanCalculation } from '../../../core/models/health-plan-selection.model';
import { TravelPlanSelection, TripType, Continent } from '../../../core/models/travel-plan-selection.model';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';
import { HealthPlanSelectorComponent } from '../../blocks/health-plan-selector/health-plan-selector.component';
import { HealthPlanPreviewComponent } from '../../blocks/health-plan-preview/health-plan-preview.component';
import { AgeRestrictionComponent } from '../../blocks/age-restriction/age-restriction.component';
import { TravelPlanPreviewComponent } from '../../blocks/travel-plan-preview/travel-plan-preview.component';
import { TravelDurationRestrictionComponent } from '../../blocks/travel-duration-restriction/travel-duration-restriction.component';
import { PolicyTypeSelectorComponent } from '../../blocks/policy-type-selector/policy-type-selector.component';
import { ContinentSelectorComponent } from '../../blocks/continent-selector/continent-selector.component';

@Component({
  selector: 'app-policy-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDividerModule,
    PageHeaderComponent,
    StatusLabelPipe,
    HealthPlanSelectorComponent,
    HealthPlanPreviewComponent,
    AgeRestrictionComponent,
    TravelPlanPreviewComponent,
    TravelDurationRestrictionComponent,
    PolicyTypeSelectorComponent,
    ContinentSelectorComponent,
  ],
  templateUrl: './policy-create.component.html',
  styleUrl: './policy-create.component.css',
})
export class PolicyCreateComponent implements OnInit, OnDestroy {
  service      = inject(PoliciesCoreService);
  healthSvc    = inject(HealthPlansService);
  travelSvc    = inject(TravelPlansService);
  router       = inject(Router);
  snackBar     = inject(MatSnackBar);
  fb           = inject(FormBuilder);

  private destroy$ = new Subject<void>();

  typeOptions: PolicyType[] = ['Life', 'Health', 'Vehicle', 'Home', 'Travel'];
  submitting = false;

  // ── Type selection (Step 0) ───────────────────────────────────────────────
  selectedType = signal<PolicyType | null>(null);

  // ── Health state ──────────────────────────────────────────────────────────
  healthPlans       = signal<HealthPlan[]>([]);
  loadingPlans      = signal(false);
  selectedPlanId    = signal<string | null>(null);
  healthCalculation = signal<HealthPlanCalculation | null>(null);
  ageRestricted     = signal(false);
  isHealthType      = signal(false);

  // ── Travel state ──────────────────────────────────────────────────────────
  isTravelType      = signal(false);
  tripType          = signal<TripType | null>(null);
  continent         = signal<Continent | null>(null);
  durationDays      = signal<number | null>(null);
  travelCalculation = signal<TravelPlanSelection | null>(null);
  durationExceeded  = computed(() => {
    const d = this.durationDays();
    return d !== null && d > 180;
  });

  insuredForm = this.fb.group({
    firstName:    ['', Validators.required],
    lastName:     ['', Validators.required],
    docuemntType: ['', Validators.required],
    documentId:   ['', Validators.required],
    birthDate:    [null as Date | null, Validators.required],
    email:        ['', [Validators.required, Validators.email]],
    phone:        [''],
  });

  coverageForm = this.fb.group({
    type:           ['Life' as PolicyType, Validators.required],
    insuredAmount:  [null as number | null, [Validators.min(1)]],
    monthlyPremium: [null as number | null, [Validators.required, Validators.min(1)]],
    startDate:      [null as Date | null, Validators.required],
    endDate:        [null as Date | null, Validators.required],
  });

  ngOnInit(): void {
    combineLatest([
      this.coverageForm.get('startDate')!.valueChanges,
      this.coverageForm.get('endDate')!.valueChanges,
    ]).pipe(takeUntil(this.destroy$))
      .subscribe(([start, end]) => {
        if (start && end) {
          const s = start as Date;
          const e = end as Date;
          const days = Math.round((e.getTime() - s.getTime()) / 86_400_000) + 1;
          this.durationDays.set(days > 0 ? days : null);
          this.travelCalculation.set(null);
          if (this.isTravelType() && !this.durationExceeded()) {
            this.tryCalculateTravel();
          }
        } else {
          this.durationDays.set(null);
          this.travelCalculation.set(null);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onTypeCardSelected(type: PolicyType): void {
    this.selectedType.set(type);
    this.coverageForm.patchValue({ type });
    this.onTypeChange(type);
  }

  onContinentSelected(continent: Continent | null): void {
    this.continent.set(continent);
    this.tryCalculateTravel();
  }

  onTypeChange(type: PolicyType): void {
    this.isHealthType.set(type === 'Health');
    this.isTravelType.set(type === 'Travel');

    this.selectedPlanId.set(null);
    this.healthCalculation.set(null);
    this.ageRestricted.set(false);
    this.tripType.set(null);
    this.continent.set(null);
    this.durationDays.set(null);
    this.travelCalculation.set(null);

    const monthlyPremiumCtrl = this.coverageForm.get('monthlyPremium')!;
    if (type === 'Travel') {
      monthlyPremiumCtrl.clearValidators();
      monthlyPremiumCtrl.setValue(null);
    } else {
      monthlyPremiumCtrl.setValidators([Validators.required, Validators.min(1)]);
    }
    monthlyPremiumCtrl.updateValueAndValidity();

    if (type === 'Health') {
      this.loadHealthPlans();
      this.checkAgeRestriction();
    }
  }

  onTripTypeChange(type: TripType): void {
    this.tripType.set(type);
    this.continent.set(null);
    this.travelCalculation.set(null);
    this.tryCalculateTravel();
  }

  onContinentChange(continent: Continent): void {
    this.continent.set(continent);
    this.tryCalculateTravel();
  }

  private tryCalculateTravel(): void {
    const type = this.tripType();
    const days = this.durationDays();
    if (!type || !days || days < 1 || days > 365) return;
    if (type === 'Internacional' && !this.continent()) return;

    this.travelSvc.calculate(type, days, this.continent() ?? undefined)
      .pipe(takeUntil(this.destroy$), catchError(() => EMPTY))
      .subscribe(calc => this.travelCalculation.set(calc));
  }

  onPlanSelected(plan: HealthPlan): void {
    this.selectedPlanId.set(plan.planId);
    this.healthCalculation.set(null);
    this.recalculate(plan.planId);
  }

  private loadHealthPlans(): void {
    if (this.healthPlans().length > 0) return;
    this.loadingPlans.set(true);
    this.healthSvc.getPlans()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: plans => {
          this.healthPlans.set(plans);
          this.loadingPlans.set(false);
        },
        error: () => this.loadingPlans.set(false),
      });
  }

  private checkAgeRestriction(): void {
    const birthDate = this.insuredForm.value.birthDate;
    if (!birthDate) return;

    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) age--;

    this.ageRestricted.set(age >= 74);
  }

  private recalculate(planId: string): void {
    const birthDate = this.insuredForm.value.birthDate;
    if (!birthDate) return;

    this.healthSvc.calculate(planId, this.toDateStr(birthDate))
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.healthCalculation.set(null);
          return EMPTY;
        })
      )
      .subscribe(calc => this.healthCalculation.set(calc));
  }

  submit(): void {
    if (this.insuredForm.invalid || this.coverageForm.invalid) return;
    this.submitting = true;

    const iv       = this.insuredForm.value;
    const cv       = this.coverageForm.value;
    const isHealth = this.isHealthType();
    const isTravel = this.isTravelType();
    const calc     = this.travelCalculation();

    const startDate = cv.startDate ? this.toDateStr(cv.startDate) : '';
    const endDate   = cv.endDate
      ? this.toDateStr(cv.endDate)
      : (cv.startDate && this.durationDays()
          ? this.toDateStr(new Date(cv.startDate!.getTime() + (this.durationDays()! - 1) * 86_400_000))
          : '');

    this.service.create({
      type: cv.type!,
      insured: {
        firstName:    iv.firstName!,
        lastName:     iv.lastName!,
        documentType: iv.docuemntType!,
        documentId:   iv.documentId!,
        birthDate:    this.toDateStr(iv.birthDate!),
        email:        iv.email!,
        phone:        iv.phone ?? '',
      },
      coveragePeriod: { startDate, endDate },
      insuredAmount:  isHealth ? (this.healthCalculation()?.finalAmount ?? 0)
                    : isTravel ? (calc?.totalPriceCop ?? 0)
                    : cv.insuredAmount!,
      monthlyPremium: isTravel ? (calc?.totalPriceCop ?? 0) : cv.monthlyPremium!,
      ...(isHealth && this.selectedPlanId() ? { healthPlanId: this.selectedPlanId()! } : {}),
      ...(isTravel && this.tripType() ? {
        tripType:     this.tripType()!,
        continent:    this.continent() ?? undefined,
        durationDays: this.durationDays()!,
      } : {}),
    }).subscribe({
      next: () => {
        this.snackBar.open('Póliza creada exitosamente', 'Cerrar', { duration: 3000 });
        this.router.navigate(['/policies']);
      },
      error: () => { this.submitting = false; },
    });
  }

  private toDateStr(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}
