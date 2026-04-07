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
import { PoliciesService } from '../services/policies.service';
import { HealthPlansService } from '../services/health-plans.service';
import { TravelPlansService } from '../services/travel-plans.service';
import {
  HealthPlanDto, HealthPlanCalculationDto, PolicyType,
  TripType, Continent, TravelPlanCalculationDto
} from '../models/policy.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';
import { HealthPlanSelectorComponent } from '../components/health-plan-selector/health-plan-selector.component';
import { HealthPlanPreviewComponent } from '../components/health-plan-preview/health-plan-preview.component';
import { AgeRestrictionComponent } from '../components/age-restriction/age-restriction.component';
import { TravelPlanPreviewComponent } from '../components/travel-plan-preview/travel-plan-preview.component';
import { TravelDurationRestrictionComponent } from '../components/travel-duration-restriction/travel-duration-restriction.component';

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
  ],
  template: `
    <app-page-header
      title="Nueva Póliza"
      actionLabel="Cancelar"
      actionIcon="close"
      (action)="router.navigate(['/policies'])" />

    <mat-stepper [linear]="true" #stepper class="stepper">

      <!-- ── Paso 1: Asegurado ──────────────────────────────────────────── -->
      <mat-step [stepControl]="insuredForm" label="Asegurado">
        <form [formGroup]="insuredForm">
          <p class="step-hint">Ingresa los datos personales del titular de la póliza.</p>

          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Primer Nombre</mat-label>
              <mat-icon matPrefix>person</mat-icon>
              <input matInput formControlName="name" placeholder="Ej. Juan" />
              @if (insuredForm.get('firstName')?.touched && insuredForm.get('firstName')?.hasError('required')) {
                <mat-error>El nombre es obligatorio</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Apellido</mat-label>
              <mat-icon matPrefix>person</mat-icon>
              <input matInput formControlName="name" placeholder="Ej. Pérez" />
              @if (insuredForm.get('lastName')?.touched && insuredForm.get('LastName')?.hasError('required')) {
                <mat-error>El apellido es obligatorio</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Ingrese tipo de documento</mat-label>
              <mat-icon matPrefix>person</mat-icon>
              <input matInput formControlName="name" placeholder="Ej. CC/ PP/ CE" />
              @if (insuredForm.get('documentType')?.touched && insuredForm.get('documentType')?.hasError('required')) {
                <mat-error>El nombre es obligatorio</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Número de documento</mat-label>
              <mat-icon matPrefix>badge</mat-icon>
              <input matInput formControlName="documentId" placeholder="Ej. 123456789" />
              @if (insuredForm.get('documentId')?.touched && insuredForm.get('documentId')?.hasError('required')) {
                <mat-error>El documento es obligatorio</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Fecha de nacimiento</mat-label>
              <mat-icon matPrefix>cake</mat-icon>
              <input matInput [matDatepicker]="birthPicker" formControlName="birthDate"
                     placeholder="DD/MM/AAAA" readonly />
              <mat-datepicker-toggle matIconSuffix [for]="birthPicker" />
              <mat-datepicker #birthPicker startView="multi-year" />
              @if (insuredForm.get('birthDate')?.touched && insuredForm.get('birthDate')?.hasError('required')) {
                <mat-error>La fecha de nacimiento es obligatoria</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <mat-icon matPrefix>email</mat-icon>
              <input matInput formControlName="email" type="email" placeholder="correo@ejemplo.com" />
              @if (insuredForm.get('email')?.touched && insuredForm.get('email')?.hasError('required')) {
                <mat-error>El email es obligatorio</mat-error>
              }
              @if (insuredForm.get('email')?.touched && insuredForm.get('email')?.hasError('email')) {
                <mat-error>Formato de email inválido</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Teléfono</mat-label>
              <mat-icon matPrefix>phone</mat-icon>
              <input matInput formControlName="phone" placeholder="Ej. +57 300 000 0000" />
            </mat-form-field>
          </div>

          <div class="step-actions">
            <button mat-raised-button color="primary" matStepperNext [disabled]="insuredForm.invalid">
              Continuar <mat-icon iconPositionEnd>arrow_forward</mat-icon>
            </button>
          </div>
        </form>
      </mat-step>

      <!-- ── Paso 2: Cobertura ─────────────────────────────────────────── -->
      <mat-step [stepControl]="coverageForm" label="Cobertura">
        <form [formGroup]="coverageForm">
          <p class="step-hint">Define el tipo, periodo y montos de cobertura.</p>

          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Tipo de póliza</mat-label>
              <mat-icon matPrefix>category</mat-icon>
              <mat-select formControlName="type" (valueChange)="onTypeChange($event)">
                @for (t of typeOptions; track t) {
                  <mat-option [value]="t">{{ t | statusLabel }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <!-- ── Health: bloqueo por edad ≥ 74 ────────────────────────── -->
            @if (isHealthType() && ageRestricted()) {
              <div class="full-width">
                <app-age-restriction />
              </div>
            }

            <!-- ── Health: selector de planes ────────────────────────────── -->
            @if (isHealthType() && !ageRestricted()) {
              <div class="full-width">
                <p class="field-label">Selecciona un plan de salud</p>
                @if (loadingPlans()) {
                  <p class="loading-text">Cargando planes...</p>
                } @else {
                  <app-health-plan-selector
                    [plans]="healthPlans()"
                    [selectedPlanId]="selectedPlanId()"
                    (planSelected)="onPlanSelected($event)" />
                }
                @if (healthCalculation()) {
                  <app-health-plan-preview [calculation]="healthCalculation()" />
                }
              </div>
            }

            <!-- ── Travel: tipo de viaje ──────────────────────────────────── -->
            @if (isTravelType()) {
              <mat-form-field appearance="outline" data-testid="trip-type-select">
                <mat-label>Tipo de viaje</mat-label>
                <mat-icon matPrefix>flight</mat-icon>
                <mat-select [value]="tripType()" (valueChange)="onTripTypeChange($event)">
                  <mat-option value="Nacional">Nacional</mat-option>
                  <mat-option value="Internacional">Internacional</mat-option>
                </mat-select>
              </mat-form-field>

              <!-- Continente — solo Internacional -->
              @if (tripType() === 'Internacional') {
                <mat-form-field appearance="outline" data-testid="continent-select">
                  <mat-label>Continente de destino</mat-label>
                  <mat-icon matPrefix>public</mat-icon>
                  <mat-select [value]="continent()" (valueChange)="onContinentChange($event)">
                    <mat-option value="America">América</mat-option>
                    <mat-option value="Europe">Europa</mat-option>
                    <mat-option value="Africa">África</mat-option>
                    <mat-option value="Asia">Asia</mat-option>
                    <mat-option value="Oceania">Oceanía</mat-option>
                  </mat-select>
                </mat-form-field>
              }

              <mat-form-field appearance="outline">
                <mat-label>Días de cobertura</mat-label>
                <mat-icon matPrefix>date_range</mat-icon>
                <input matInput readonly data-testid="duration-days-input"
                       [value]="durationDays() !== null ? durationDays() : '—'" />
                <mat-hint>Calculado automáticamente según las fechas seleccionadas</mat-hint>
              </mat-form-field>

              <!-- Banner: duración excedida -->
              @if (durationExceeded()) {
                <div class="full-width">
                  <app-travel-duration-restriction />
                </div>
              }

              <!-- Preview del cálculo -->
              @if (travelCalculation() && !durationExceeded()) {
                <div class="full-width">
                  <app-travel-plan-preview [calculation]="travelCalculation()" />
                </div>
              }
            }

            <!-- ── Monto manual — solo para tipos no-Health y no-Travel ─── -->
            @if (!isHealthType() && !isTravelType()) {
              <mat-form-field appearance="outline">
                <mat-label>Monto asegurado (COP)</mat-label>
                <mat-icon matPrefix>attach_money</mat-icon>
                <input matInput formControlName="insuredAmount" type="number" min="1" placeholder="Ej. 50000" />
                @if (coverageForm.get('insuredAmount')?.touched && coverageForm.get('insuredAmount')?.hasError('required')) {
                  <mat-error>El monto es obligatorio</mat-error>
                }
                @if (coverageForm.get('insuredAmount')?.touched && coverageForm.get('insuredAmount')?.hasError('min')) {
                  <mat-error>El monto debe ser mayor a 0</mat-error>
                }
              </mat-form-field>
            }

            <!-- ── Prima mensual — solo para tipos no-Travel ─────────────── -->
            @if (!isTravelType()) {
              <mat-form-field appearance="outline">
                <mat-label>Prima mensual (COP)</mat-label>
                <mat-icon matPrefix>payments</mat-icon>
                <input matInput formControlName="monthlyPremium" type="number" min="1" placeholder="Ej. 150" />
                @if (coverageForm.get('monthlyPremium')?.touched && coverageForm.get('monthlyPremium')?.hasError('required')) {
                  <mat-error>La prima mensual es obligatoria</mat-error>
                }
                @if (coverageForm.get('monthlyPremium')?.touched && coverageForm.get('monthlyPremium')?.hasError('min')) {
                  <mat-error>La prima debe ser mayor a 0</mat-error>
                }
              </mat-form-field>
            }

            <mat-form-field appearance="outline">
              <mat-label>Inicio de cobertura</mat-label>
              <mat-icon matPrefix>event</mat-icon>
              <input matInput [matDatepicker]="startPicker" formControlName="startDate"
                     placeholder="DD/MM/AAAA" readonly />
              <mat-datepicker-toggle matIconSuffix [for]="startPicker" />
              <mat-datepicker #startPicker />
              @if (coverageForm.get('startDate')?.touched && coverageForm.get('startDate')?.hasError('required')) {
                <mat-error>La fecha de inicio es obligatoria</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Fin de cobertura</mat-label>
              <mat-icon matPrefix>event_busy</mat-icon>
              <input matInput [matDatepicker]="endPicker" formControlName="endDate"
                     placeholder="DD/MM/AAAA" readonly />
              <mat-datepicker-toggle matIconSuffix [for]="endPicker" />
              <mat-datepicker #endPicker />
              @if (coverageForm.get('endDate')?.touched && coverageForm.get('endDate')?.hasError('required')) {
                <mat-error>La fecha de fin es obligatoria</mat-error>
              }
            </mat-form-field>
          </div>

          <div class="step-actions">
            <button mat-stroked-button matStepperPrevious>
              <mat-icon>arrow_back</mat-icon> Anterior
            </button>
            <button mat-raised-button color="primary" matStepperNext
                    [disabled]="coverageForm.invalid
                      || (isHealthType() && (!selectedPlanId() || !healthCalculation() || ageRestricted()))
                      || (isTravelType() && (!tripType() || !durationDays() || durationExceeded() || !travelCalculation()))
                      || (isTravelType() && tripType() === 'Internacional' && !continent())">
              Continuar <mat-icon iconPositionEnd>arrow_forward</mat-icon>
            </button>
          </div>
        </form>
      </mat-step>

      <!-- ── Paso 3: Confirmación ──────────────────────────────────────── -->
      <mat-step label="Confirmación">
        <p class="step-hint">Revisa el resumen antes de crear la póliza.</p>

        <div class="summary-card">
          <p class="summary-section-title">Datos del asegurado</p>
          <mat-divider />
          <div class="summary-grid">
            <span class="label">Nombre</span>
            <span>{{ insuredForm.value.firstName }}</span>
            <span class="label">Apellido</span>
            <span>{{ insuredForm.value.lastName }}</span>
            <span class="label">Tipo de docuemnto</span>
            <span>{{ insuredForm.value.docuemntType }}</span>
            <span class="label">Documento</span>
            <span>{{ insuredForm.value.documentId }}</span>
            <span class="label">Nacimiento</span>
            <span>{{ insuredForm.value.birthDate | date:'dd/MM/yyyy' }}</span>
            <span class="label">Email</span>
            <span>{{ insuredForm.value.email }}</span>
            @if (insuredForm.value.phone) {
              <span class="label">Teléfono</span>
              <span>{{ insuredForm.value.phone }}</span>
            }
          </div>

          <p class="summary-section-title" style="margin-top:16px">Cobertura</p>
          <mat-divider />
          <div class="summary-grid">
            <span class="label">Tipo</span>
            <span>{{ coverageForm.value.type ?? '' | statusLabel }}</span>

            @if (isHealthType() && healthCalculation()) {
              <span class="label">Plan de salud</span>
              <span>{{ healthCalculation()!.planName }}</span>
              <span class="label">Monto asegurado</span>
              <span>{{ healthCalculation()!.finalAmount | currency:'COP':'symbol':'1.0-0' }}</span>
              <span class="label">Factor edad</span>
              <span>{{ healthCalculation()!.ageFactorPercentage }}%</span>
            } @else if (isTravelType() && travelCalculation()) {
              <span class="label">Tipo de viaje</span>
              <span>{{ travelCalculation()!.tripType }}</span>
              @if (travelCalculation()!.continent) {
                <span class="label">Continente</span>
                <span>{{ travelCalculation()!.continent }}</span>
              }
              <span class="label">Días de cobertura</span>
              <span>{{ travelCalculation()!.durationDays }}</span>
              <span class="label">Total póliza (COP)</span>
              <span>{{ travelCalculation()!.totalPriceCop | currency:'COP':'symbol':'1.0-0' }}</span>
            } @else {
              <span class="label">Monto asegurado</span>
              <span>{{ coverageForm.value.insuredAmount | currency:'COP':'symbol':'1.0-0' }}</span>
            }

            @if (!isTravelType()) {
              <span class="label">Prima mensual</span>
              <span>{{ coverageForm.value.monthlyPremium | currency:'COP':'symbol':'1.0-0' }}</span>
            }
            <span class="label">Inicio</span>
            <span>{{ coverageForm.value.startDate | date:'dd/MM/yyyy' }}</span>
            <span class="label">Fin</span>
            <span>{{ coverageForm.value.endDate | date:'dd/MM/yyyy' }}</span>
          </div>
        </div>

        <div class="step-actions">
          <button mat-stroked-button matStepperPrevious>
            <mat-icon>arrow_back</mat-icon> Anterior
          </button>
          <button mat-raised-button color="primary" [disabled]="submitting" (click)="submit()">
            <mat-icon>check_circle</mat-icon>
            @if (submitting) { Creando... } @else { Confirmar y Crear }
          </button>
        </div>
      </mat-step>
    </mat-stepper>
  `,
  styles: [`
    .stepper { background: transparent; }

    .step-hint {
      color: var(--mat-sys-on-surface-variant, #666);
      font-size: 14px;
      margin: 8px 0 16px;
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
      padding: 8px 0;
    }

    .full-width { grid-column: 1 / -1; }

    .field-label {
      font-size: 13px;
      font-weight: 500;
      color: var(--mat-sys-on-surface-variant, #666);
      margin: 0 0 8px;
    }

    .loading-text {
      font-size: 13px;
      color: var(--mat-sys-on-surface-variant, #666);
    }

    .step-actions {
      display: flex;
      gap: 12px;
      margin-top: 20px;
      padding-bottom: 8px;
    }

    .summary-card {
      background: var(--mat-sys-surface-container, #f5f5f5);
      border-radius: 8px;
      padding: 20px;
      margin: 8px 0 16px;
    }

    .summary-section-title {
      font-weight: 600;
      font-size: 13px;
      text-transform: uppercase;
      letter-spacing: .5px;
      color: var(--mat-sys-primary, #6750a4);
      margin: 0 0 8px;
    }

    .summary-grid {
      display: grid;
      grid-template-columns: 140px 1fr;
      gap: 8px 16px;
      margin-top: 12px;
    }

    .summary-grid .label {
      color: var(--mat-sys-on-surface-variant, #666);
      font-size: 13px;
    }
  `],
})
export class PolicyCreateComponent implements OnInit, OnDestroy {
  service      = inject(PoliciesService);
  healthSvc    = inject(HealthPlansService);
  travelSvc    = inject(TravelPlansService);
  router       = inject(Router);
  snackBar     = inject(MatSnackBar);
  fb           = inject(FormBuilder);

  private destroy$ = new Subject<void>();

  typeOptions: PolicyType[] = ['Life', 'Health', 'Vehicle', 'Home', 'Travel'];
  submitting   = false;

  // ── Health state ──────────────────────────────────────────────────────────
  healthPlans       = signal<HealthPlanDto[]>([]);
  loadingPlans      = signal(false);
  selectedPlanId    = signal<string | null>(null);
  healthCalculation = signal<HealthPlanCalculationDto | null>(null);
  ageRestricted     = signal(false);
  isHealthType      = signal(false);

  // ── Travel state ──────────────────────────────────────────────────────────
  isTravelType      = signal(false);
  tripType          = signal<TripType | null>(null);
  continent         = signal<Continent | null>(null);
  durationDays      = signal<number | null>(null);
  travelCalculation = signal<TravelPlanCalculationDto | null>(null);
  durationExceeded  = computed(() => {
    const d = this.durationDays();
    return d !== null && d > 180;
  });

  insuredForm = this.fb.group({
    firstName:       ['', Validators.required],
    lastName:        ['', Validators.required],
    docuemntType:    ['', Validators.required],
    documentId:      ['', Validators.required],
    birthDate:  [null as Date | null, Validators.required],
    email:      ['', [Validators.required, Validators.email]],
    phone:      [''],
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
          const days = Math.round((end.getTime() - start.getTime()) / 86_400_000) + 1;
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

  onTypeChange(type: PolicyType): void {
    this.isHealthType.set(type === 'Health');
    this.isTravelType.set(type === 'Travel');

    // reset health
    this.selectedPlanId.set(null);
    this.healthCalculation.set(null);
    this.ageRestricted.set(false);

    // reset travel
    this.tripType.set(null);
    this.continent.set(null);
    this.durationDays.set(null);
    this.travelCalculation.set(null);

    if (type === 'Health') {
      this.loadHealthPlans();
      this.checkAgeRestriction();
    }
  }

  // ── Travel handlers ───────────────────────────────────────────────────────

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

  onPlanSelected(plan: HealthPlanDto): void {
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

    const dateStr = this.toDateStr(birthDate);

    this.healthSvc.calculate(planId, dateStr)
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.healthCalculation.set(null);
          return EMPTY;
        })
      )
      .subscribe(calc => this.healthCalculation.set(calc));
  }

  submit() {
    if (this.insuredForm.invalid || this.coverageForm.invalid) return;
    this.submitting = true;

    const iv       = this.insuredForm.value;
    const cv       = this.coverageForm.value;
    const isHealth = this.isHealthType();
    const isTravel = this.isTravelType();
    const calc     = this.travelCalculation();

    // Para Travel: las fechas de cobertura coinciden con los días del viaje
    const startDate = cv.startDate ? this.toDateStr(cv.startDate) : '';
    const endDate   = cv.endDate
      ? this.toDateStr(cv.endDate)
      : (cv.startDate && this.durationDays()
          ? this.toDateStr(new Date(cv.startDate!.getTime() + (this.durationDays()! - 1) * 86_400_000))
          : '');

    this.service.createPolicy({
      type: cv.type!,
      insured: {
        firstName:       iv.firstName!,
        lastName:        iv.lastName!,
        documentType:   iv.docuemntType!,
        documentId: iv.documentId!,
        birthDate:  this.toDateStr(iv.birthDate!),
        email:      iv.email!,
        phone:      iv.phone ?? '',
      },
      coveragePeriod: { startDate, endDate },
      insuredAmount:  isHealth ? (this.healthCalculation()?.finalAmount ?? 0)
                    : isTravel ? (calc?.totalPriceCop ?? 0)
                    : cv.insuredAmount!,
      monthlyPremium: isTravel ? (calc?.totalPriceCop ?? 0) : cv.monthlyPremium!,
      ...(isHealth && this.selectedPlanId() ? { healthPlanId: this.selectedPlanId()! } : {}),
      ...(isTravel && this.tripType() ? {
        tripType:    this.tripType()!,
        continent:   this.continent() ?? undefined,
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
