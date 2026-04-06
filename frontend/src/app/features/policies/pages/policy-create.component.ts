import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
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
import { PolicyType } from '../models/policy.model';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';

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
  ],
  template: `
    <app-page-header
      title="Nueva Póliza"
      actionLabel="Cancelar"
      actionIcon="close"
      (action)="router.navigate(['/policies'])" />

    <mat-stepper [linear]="true" #stepper class="stepper">

      <!-- ── Paso 1: Asegurado ────────────────────────────────────────── -->
      <mat-step [stepControl]="insuredForm" label="Asegurado">
        <form [formGroup]="insuredForm">
          <p class="step-hint">Ingresa los datos personales del titular de la póliza.</p>

          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Nombre completo</mat-label>
              <mat-icon matPrefix>person</mat-icon>
              <input matInput formControlName="name" placeholder="Ej. Juan Pérez" />
              @if (insuredForm.get('name')?.touched && insuredForm.get('name')?.hasError('required')) {
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
              <mat-select formControlName="type">
                @for (t of typeOptions; track t) {
                  <mat-option [value]="t">{{ t | statusLabel }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Monto asegurado (USD)</mat-label>
              <mat-icon matPrefix>attach_money</mat-icon>
              <input matInput formControlName="insuredAmount" type="number" min="1" placeholder="Ej. 50000" />
              @if (coverageForm.get('insuredAmount')?.touched && coverageForm.get('insuredAmount')?.hasError('required')) {
                <mat-error>El monto es obligatorio</mat-error>
              }
              @if (coverageForm.get('insuredAmount')?.touched && coverageForm.get('insuredAmount')?.hasError('min')) {
                <mat-error>El monto debe ser mayor a 0</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Prima mensual (USD)</mat-label>
              <mat-icon matPrefix>payments</mat-icon>
              <input matInput formControlName="monthlyPremium" type="number" min="1" placeholder="Ej. 150" />
              @if (coverageForm.get('monthlyPremium')?.touched && coverageForm.get('monthlyPremium')?.hasError('required')) {
                <mat-error>La prima mensual es obligatoria</mat-error>
              }
              @if (coverageForm.get('monthlyPremium')?.touched && coverageForm.get('monthlyPremium')?.hasError('min')) {
                <mat-error>La prima debe ser mayor a 0</mat-error>
              }
            </mat-form-field>

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
            <button mat-raised-button color="primary" matStepperNext [disabled]="coverageForm.invalid">
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
            <span>{{ insuredForm.value.name }}</span>
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
            <span class="label">Monto asegurado</span>
            <span>{{ coverageForm.value.insuredAmount | currency:'USD' }}</span>
            <span class="label">Prima mensual</span>
            <span>{{ coverageForm.value.monthlyPremium | currency:'USD' }}</span>
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
export class PolicyCreateComponent {
  service = inject(PoliciesService);
  router = inject(Router);
  snackBar = inject(MatSnackBar);
  fb = inject(FormBuilder);

  typeOptions: PolicyType[] = ['Life', 'Health', 'Vehicle', 'Home', 'Travel'];
  submitting = false;

  insuredForm = this.fb.group({
    name:       ['', Validators.required],
    documentId: ['', Validators.required],
    birthDate:  [null as Date | null, Validators.required],
    email:      ['', [Validators.required, Validators.email]],
    phone:      [''],
  });

  coverageForm = this.fb.group({
    type:           ['Life' as PolicyType, Validators.required],
    insuredAmount:  [null as number | null, [Validators.required, Validators.min(1)]],
    monthlyPremium: [null as number | null, [Validators.required, Validators.min(1)]],
    startDate:      [null as Date | null, Validators.required],
    endDate:        [null as Date | null, Validators.required],
  });

  submit() {
    if (this.insuredForm.invalid || this.coverageForm.invalid) return;
    this.submitting = true;

    const iv = this.insuredForm.value;
    const cv = this.coverageForm.value;

    const toDateStr = (d: Date | null | undefined) =>
      d ? `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}` : '';

    this.service.createPolicy({
      type: cv.type!,
      insured: {
        name:       iv.name!,
        documentId: iv.documentId!,
        birthDate:  toDateStr(iv.birthDate),
        email:      iv.email!,
        phone:      iv.phone ?? '',
      },
      coveragePeriod: {
        startDate: toDateStr(cv.startDate),
        endDate:   toDateStr(cv.endDate),
      },
      insuredAmount:  cv.insuredAmount!,
      monthlyPremium: cv.monthlyPremium!,
    }).subscribe({
      next: () => {
        this.snackBar.open('Póliza creada exitosamente', 'Cerrar', { duration: 3000 });
        this.router.navigate(['/policies']);
      },
      error: () => { this.submitting = false; },
    });
  }
}
