import { Component, inject, signal, computed, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
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
import { LifePlansService } from '../../../core/service/life-plans.service';
import { VehiclePlansService } from '../../../core/service/vehicle-plans.service';
import { PolicyType } from '../../../core/models/policy.model';
import { HealthPlan, HealthPlanCalculation } from '../../../core/models/health-plan-selection.model';
import { LifePlan, LifePlanCalculation } from '../../../core/models/life-plan-selection.model';
import { TravelPlanSelection, TripType, Continent } from '../../../core/models/travel-plan-selection.model';
import { VehiclePlanOption, VehicleQuotationResult } from '../../../core/models/vehicle-plan-selection.model';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';
import { HealthPlanSelectorComponent } from '../../blocks/health-plan-selector/health-plan-selector.component';
import { HealthPlanPreviewComponent } from '../../blocks/health-plan-preview/health-plan-preview.component';
import { LifePlanSelectorComponent } from '../../blocks/life-plan-selector/life-plan-selector.component';
import { LifePlanPreviewComponent } from '../../blocks/life-plan-preview/life-plan-preview.component';
import { LifeAgeRestrictionComponent } from '../../blocks/life-age-restriction/life-age-restriction.component';
import { AgeRestrictionComponent } from '../../blocks/age-restriction/age-restriction.component';
import { TravelPlanPreviewComponent } from '../../blocks/travel-plan-preview/travel-plan-preview.component';
import { TravelDurationRestrictionComponent } from '../../blocks/travel-duration-restriction/travel-duration-restriction.component';
import { PolicyTypeSelectorComponent } from '../../blocks/policy-type-selector/policy-type-selector.component';
import { ContinentSelectorComponent } from '../../blocks/continent-selector/continent-selector.component';
import { VehicleDataFormComponent, VehicleDataFormValue } from '../../blocks/vehicle-data-form/vehicle-data-form.component';
import { VehiclePlanSelectorComponent } from '../../blocks/vehicle-plan-selector/vehicle-plan-selector.component';
import { VehiclePlanPreviewComponent } from '../../blocks/vehicle-plan-preview/vehicle-plan-preview.component';
import { CitiesService } from '../../../core/service/cities.service';
import { CityOption } from '../../../core/models/city.model';
import { DocumentType, DOCUMENT_TYPE_OPTIONS } from '../../../core/models/insured-person.model';
import { ThousandsSeparatorDirective } from '../../../../../shared/directives/thousands-separator.directive';
import { HomePlansService } from '../../../core/service/home-plans.service';
import { HomePlanPackage, HomeQuotationResult, HomeDataFormValue } from '../../../core/models/home-plan-selection.model';
import { HomeDataFormComponent } from '../../blocks/home-data-form/home-data-form.component';
import { HomePlanPreviewComponent } from '../../blocks/home-plan-preview/home-plan-preview.component';

function documentIdValidatorFn(type: DocumentType): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) return { required: true };
    if (['CC', 'TI', 'RC'].includes(type)) {
      const raw = value.replace(/\./g, '');
      return /^\d{1,10}$/.test(raw) ? null : { invalidFormat: true };
    }
    if (['CE', 'PP'].includes(type)) {
      return /^[A-Za-z0-9]{1,11}$/.test(value) ? null : { invalidFormat: true };
    }
    return null;
  };
}

function startDateNotInPastValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value as Date | null;
  if (!value) return null;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return value < today ? { pastDate: true } : null;
}

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
    LifePlanSelectorComponent,
    LifePlanPreviewComponent,
    LifeAgeRestrictionComponent,
    AgeRestrictionComponent,
    TravelPlanPreviewComponent,
    TravelDurationRestrictionComponent,
    PolicyTypeSelectorComponent,
    ContinentSelectorComponent,
    VehicleDataFormComponent,
    VehiclePlanSelectorComponent,
    VehiclePlanPreviewComponent,
    ThousandsSeparatorDirective,
    HomeDataFormComponent,
    HomePlanPreviewComponent,
  ],
  templateUrl: './policy-create.component.html',
  styleUrl: './policy-create.component.css',
})
export class PolicyCreateComponent implements OnInit, OnDestroy {
  service      = inject(PoliciesCoreService);
  healthSvc    = inject(HealthPlansService);
  travelSvc    = inject(TravelPlansService);
  lifeSvc      = inject(LifePlansService);
  vehicleSvc   = inject(VehiclePlansService);
  citiesSvc    = inject(CitiesService);
  router       = inject(Router);
  snackBar     = inject(MatSnackBar);
  fb           = inject(FormBuilder);

  readonly DOCUMENT_TYPE_OPTIONS = DOCUMENT_TYPE_OPTIONS;

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

  // ── Life state ────────────────────────────────────────────────────────────
  isLifeType         = signal(false);
  lifePlans          = signal<LifePlan[]>([]);
  loadingLifePlans   = signal(false);
  selectedLifePlanId = signal<string | null>(null);
  lifeCalculation    = signal<LifePlanCalculation | null>(null);
  ageRestrictedLife  = signal(false);

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

  // ── Cities state ──────────────────────────────────────────────────────────
  cities                  = signal<CityOption[]>([]);
  selectedCityPostalCode  = signal('');
  selectedCityDepartment  = signal('');

  // ── Home state ────────────────────────────────────────────────────────────
  isHomeType          = signal(false);
  homePackages        = signal<HomePlanPackage[]>([]);
  homeQuotation       = signal<HomeQuotationResult | null>(null);
  homePackageId       = signal<string | null>(null);
  homeQuoteLoading    = signal(false);

  homeSvc             = inject(HomePlansService);

  homeEndDate = computed<Date | null>(() => {
    if (!this.isHomeType()) return null;
    const start = this.startDateValue();
    if (!start) return null;
    return new Date(start.getTime() + 364 * 86_400_000);
  });

  // ── Vehicle state ─────────────────────────────────────────────────────────
  isVehicleType       = signal(false);
  vehicleQuotation    = signal<VehicleQuotationResult | null>(null);
  selectedVehiclePlan = signal<VehiclePlanOption | null>(null);
  vehicleQuoteLoading = signal(false);

  startDateValue    = signal<Date | null>(null);

  /** Fecha de fin derivada para pólizas de Salud (startDate + 364 días). */
  healthEndDate = computed<Date | null>(() => {
    if (!this.isHealthType()) return null;
    const start = this.startDateValue();
    if (!start) return null;
    return new Date(start.getTime() + 364 * 86_400_000);
  });

  /** Fecha de fin derivada para pólizas de Vida (startDate + 364 días). */
  lifeEndDate = computed<Date | null>(() => {
    if (!this.isLifeType()) return null;
    const start = this.startDateValue();
    if (!start) return null;
    return new Date(start.getTime() + 364 * 86_400_000);
  });

  /** Fecha de fin derivada para pólizas de Vehículo (startDate + 364 días). */
  vehicleEndDate = computed<Date | null>(() => {
    if (!this.isVehicleType()) return null;
    const start = this.startDateValue();
    if (!start) return null;
    return new Date(start.getTime() + 364 * 86_400_000);
  });

  insuredForm = this.fb.group({
    firstName:    ['', Validators.required],
    lastName:     ['', Validators.required],
    documentType: ['', Validators.required],
    documentId:   ['', Validators.required],
    birthDate:    [null as Date | null, Validators.required],
    email:        ['', [Validators.required, Validators.email]],
    phone:        [''],
    gender:       ['', Validators.required],
    city:         ['', Validators.required],
    address:      ['', Validators.required],
  });

  coverageForm = this.fb.group({
    type:           ['Life' as PolicyType, Validators.required],
    insuredAmount:  [null as number | null, [Validators.min(1)]],
    monthlyPremium: [null as number | null, [Validators.min(1)]],
    startDate:      [null as Date | null, [Validators.required, startDateNotInPastValidator]],
    endDate:        [null as Date | null, Validators.required],
  });

  onInsuredCitySelected(cityName: string): void {
    const city = this.cities().find(c => c.name === cityName);
    if (city) {
      this.selectedCityPostalCode.set(city.postalCode);
      this.selectedCityDepartment.set(city.department);
    }
  }

  onInsuredDocumentTypeChange(type: string): void {
    this.insuredForm.get('documentId')!.setValue('');
    this.insuredForm.get('documentId')!.setValidators([documentIdValidatorFn(type as DocumentType)]);
    this.insuredForm.get('documentId')!.updateValueAndValidity();
  }

  ngOnInit(): void {
    this.citiesSvc.loadCities().subscribe(cities => this.cities.set(cities));

    this.insuredForm.get('documentType')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(type => this.onInsuredDocumentTypeChange(type ?? ''));

    this.coverageForm.get('startDate')!.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(v => this.startDateValue.set(v as Date | null));

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
    this.isLifeType.set(type === 'Life');
    this.isVehicleType.set(type === 'Vehicle');
    this.isHomeType.set(type === 'Home');

    this.selectedPlanId.set(null);
    this.healthCalculation.set(null);
    this.ageRestricted.set(false);
    this.selectedLifePlanId.set(null);
    this.lifeCalculation.set(null);
    this.ageRestrictedLife.set(false);
    this.tripType.set(null);
    this.continent.set(null);
    this.durationDays.set(null);
    this.travelCalculation.set(null);
    this.vehicleQuotation.set(null);
    this.selectedVehiclePlan.set(null);
    this.homeQuotation.set(null);
    this.homePackageId.set(null);

    const monthlyPremiumCtrl = this.coverageForm.get('monthlyPremium')!;
    const endDateCtrl        = this.coverageForm.get('endDate')!;
    const insuredAmountCtrl  = this.coverageForm.get('insuredAmount')!;

    if (type === 'Health') {
      // Prima y endDate se calculan automáticamente — no son requeridos en el form
      monthlyPremiumCtrl.clearValidators();
      monthlyPremiumCtrl.setValue(null);
      endDateCtrl.clearValidators();
      endDateCtrl.setValue(null);
      insuredAmountCtrl.clearValidators();
      insuredAmountCtrl.setValue(null);
      this.loadHealthPlans();
      this.checkAgeRestriction();
    } else if (type === 'Life') {
      // Prima y endDate se calculan automáticamente (SPEC-009)
      monthlyPremiumCtrl.clearValidators();
      monthlyPremiumCtrl.setValue(null);
      endDateCtrl.clearValidators();
      endDateCtrl.setValue(null);
      insuredAmountCtrl.clearValidators();
      insuredAmountCtrl.setValue(null);
      this.loadLifePlans();
      this.checkLifeAgeRestriction();
    } else if (type === 'Vehicle') {
      // Prima, monto y endDate se calculan automáticamente (SPEC-010)
      monthlyPremiumCtrl.clearValidators();
      monthlyPremiumCtrl.setValue(null);
      endDateCtrl.clearValidators();
      endDateCtrl.setValue(null);
      insuredAmountCtrl.clearValidators();
      insuredAmountCtrl.setValue(null);
    } else if (type === 'Travel') {
      monthlyPremiumCtrl.clearValidators();
      monthlyPremiumCtrl.setValue(null);
      insuredAmountCtrl.clearValidators();
      insuredAmountCtrl.setValue(null);
      endDateCtrl.setValidators(Validators.required);
    } else if (type === 'Home') {
      monthlyPremiumCtrl.clearValidators();
      monthlyPremiumCtrl.setValue(null);
      endDateCtrl.clearValidators();
      endDateCtrl.setValue(null);
      insuredAmountCtrl.clearValidators();
      insuredAmountCtrl.setValue(null);
      if (this.homePackages().length === 0) {
        this.homeSvc.getPackages()
          .pipe(takeUntil(this.destroy$))
          .subscribe(packages => this.homePackages.set(packages));
      }
    } else {
      monthlyPremiumCtrl.setValidators([Validators.required, Validators.min(1)]);
      endDateCtrl.setValidators(Validators.required);
    }

    monthlyPremiumCtrl.updateValueAndValidity();
    endDateCtrl.updateValueAndValidity();
    insuredAmountCtrl.updateValueAndValidity();
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

  onLifePlanSelected(plan: LifePlan): void {
    this.selectedLifePlanId.set(plan.planId);
    this.lifeCalculation.set(null);
    this.recalculateLife(plan.planId);
  }

  private loadLifePlans(): void {
    if (this.lifePlans().length > 0) return;
    this.loadingLifePlans.set(true);
    this.lifeSvc.getPlans()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: plans => {
          this.lifePlans.set(plans);
          this.loadingLifePlans.set(false);
        },
        error: () => this.loadingLifePlans.set(false),
      });
  }

  private checkLifeAgeRestriction(): void {
    const birthDate = this.insuredForm.value.birthDate;
    if (!birthDate) return;

    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const m = today.getMonth() - birthDate.getMonth();
    if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) age--;

    this.ageRestrictedLife.set(age > 65);
  }

  private recalculateLife(planId: string): void {
    const birthDate = this.insuredForm.value.birthDate;
    if (!birthDate) return;

    this.lifeSvc.calculate(planId, this.toDateStr(birthDate))
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.lifeCalculation.set(null);
          return EMPTY;
        })
      )
      .subscribe(calc => this.lifeCalculation.set(calc));
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

  onHomeQuoteRequested(data: HomeDataFormValue): void {
    this.homeQuotation.set(null);
    this.homePackageId.set(data.packageId);
    this.homeQuoteLoading.set(true);
    this.homeSvc.calculate({
      propertyValue:     data.propertyValue,
      constructionYear:  data.constructionYear,
      stratum:           data.stratum,
      occupants:         data.occupants,
      propertyType:      data.propertyType,
      selectedCoverages: data.selectedCoverages,
    }).pipe(
      takeUntil(this.destroy$),
      catchError(() => { this.homeQuoteLoading.set(false); return EMPTY; })
    ).subscribe(result => {
      this.homeQuotation.set(result);
      this.homeQuoteLoading.set(false);
    });
  }

  onVehicleQuoteRequested(data: VehicleDataFormValue): void {
    this.vehicleQuotation.set(null);
    this.selectedVehiclePlan.set(null);
    this.vehicleQuoteLoading.set(true);
    this.vehicleSvc.calculate(data.commercialValue, data.vehicleYear, data.brand)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.vehicleQuoteLoading.set(false);
          return EMPTY;
        })
      )
      .subscribe(result => {
        this.vehicleQuotation.set(result);
        this.vehicleQuoteLoading.set(false);
      });
  }

  onVehiclePlanSelected(plan: VehiclePlanOption): void {
    this.selectedVehiclePlan.set(plan);
  }

  submit(): void {
    if (this.insuredForm.invalid || this.coverageForm.invalid) return;
    this.submitting = true;

    const iv       = this.insuredForm.value;
    const cv       = this.coverageForm.value;
    const isHealth  = this.isHealthType();
    const isLife    = this.isLifeType();
    const isTravel  = this.isTravelType();
    const isVehicle = this.isVehicleType();
    const isHome    = this.isHomeType();
    const calc      = this.travelCalculation();
    const vehiclePlan  = this.selectedVehiclePlan();
    const vehicleQuote = this.vehicleQuotation();
    const homeQuote    = this.homeQuotation();

    const startDate = cv.startDate ? this.toDateStr(cv.startDate) : '';
    const endDate   = (isHealth || isLife || isVehicle || isHome) && cv.startDate
      ? this.toDateStr(new Date(cv.startDate.getTime() + 364 * 86_400_000))
      : cv.endDate
        ? this.toDateStr(cv.endDate)
        : (cv.startDate && this.durationDays()
            ? this.toDateStr(new Date(cv.startDate!.getTime() + (this.durationDays()! - 1) * 86_400_000))
            : '');

    this.service.create({
      type: cv.type!,
      insured: {
        firstName:    iv.firstName!,
        lastName:     iv.lastName!,
        documentType: iv.documentType!,
        documentId:   iv.documentId!,
        birthDate:    this.toDateStr(iv.birthDate!),
        email:        iv.email!,
        phone:        iv.phone ?? '',
        gender:       iv.gender! as 'Masculino' | 'Femenino',
        address:      iv.address!,
        cityName:     iv.city!,
        postalCode:   this.selectedCityPostalCode(),
        department:   this.selectedCityDepartment(),
      },
      coveragePeriod: { startDate, endDate },
      insuredAmount:  isHealth  ? (this.healthCalculation()?.finalAmount ?? 0)
                    : isLife    ? (this.lifeCalculation()?.deathBenefit ?? 0)
                    : isVehicle ? (vehicleQuote?.commercialValue ?? 0)
                    : isHome    ? (homeQuote?.propertyValue ?? 0)
                    : isTravel  ? (calc?.totalPriceCop ?? 0)
                    : cv.insuredAmount!,
      monthlyPremium: isHealth  ? (this.healthCalculation()?.monthlyPremium ?? 0)
                    : isLife    ? (this.lifeCalculation()?.monthlyPremium ?? 0)
                    : isVehicle ? (vehiclePlan?.monthlyPremium ?? 0)
                    : isHome    ? (homeQuote?.finalMonthlyPremium ?? 0)
                    : isTravel  ? (calc?.totalPriceCop ?? 0)
                    : cv.monthlyPremium!,
      ...(isHealth  && this.selectedPlanId()     ? { healthPlanId: this.selectedPlanId()! } : {}),
      ...(isLife    && this.selectedLifePlanId() ? { lifePlanId: this.selectedLifePlanId()! } : {}),
      ...(isVehicle && vehiclePlan && vehicleQuote ? {
        vehiclePlanId:          vehiclePlan.planId,
        vehicleCommercialValue: vehicleQuote.commercialValue,
        vehicleYear:            vehicleQuote.vehicleYear,
        vehicleBrand:           vehicleQuote.brand,
      } : {}),
      ...(isHome && homeQuote ? {
        homePlanPackageId:     this.homePackageId() ?? undefined,
        homePropertyValue:     homeQuote.propertyValue,
        homeConstructionYear:  homeQuote.constructionYear,
        homeStratum:           homeQuote.stratum,
        homeOccupants:         homeQuote.occupants,
        homePropertyType:      homeQuote.propertyType,
        homeSelectedCoverages: homeQuote.selectedCoverages,
      } : {}),
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
