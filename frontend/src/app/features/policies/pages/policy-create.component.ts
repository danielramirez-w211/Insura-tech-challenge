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
    PageHeaderComponent,
    StatusLabelPipe,
  ],
  template: `
    <app-page-header
      title="Nueva Póliza"
      actionLabel="Cancelar"
      actionIcon="close"
      (action)="router.navigate(['/policies'])" />

    <mat-stepper [linear]="true" #stepper>
      <!-- Paso 1: Datos del asegurado -->
      <mat-step [stepControl]="insuredForm" label="Datos del Asegurado">
        <form [formGroup]="insuredForm">
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Nombre completo</mat-label>
              <input matInput formControlName="name" placeholder="Ej. Juan Pérez" />
              @if (insuredForm.get('name')?.hasError('required')) {
                <mat-error>Nombre es obligatorio</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Número de documento</mat-label>
              <input matInput formControlName="documentId" placeholder="Ej. 123456789" />
              @if (insuredForm.get('documentId')?.hasError('required')) {
                <mat-error>Documento es obligatorio</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" />
              @if (insuredForm.get('email')?.hasError('email')) {
                <mat-error>Email inválido</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Teléfono</mat-label>
              <input matInput formControlName="phone" />
            </mat-form-field>
          </div>
          <div class="step-actions">
            <button mat-raised-button color="primary" matStepperNext [disabled]="insuredForm.invalid">
              Siguiente <mat-icon>arrow_forward</mat-icon>
            </button>
          </div>
        </form>
      </mat-step>

      <!-- Paso 2: Tipo y cobertura -->
      <mat-step [stepControl]="coverageForm" label="Tipo y Cobertura">
        <form [formGroup]="coverageForm">
          <div class="form-grid">
            <mat-form-field appearance="outline">
              <mat-label>Tipo de póliza</mat-label>
              <mat-select formControlName="type">
                @for (t of typeOptions; track t) {
                  <mat-option [value]="t">{{ t | statusLabel }}</mat-option>
                }
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Monto asegurado (USD)</mat-label>
              <input matInput formControlName="insuredAmount" type="number" min="1" />
              @if (coverageForm.get('insuredAmount')?.hasError('min')) {
                <mat-error>Monto debe ser mayor a 0</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Fecha inicio de cobertura</mat-label>
              <input matInput formControlName="startDate" type="date" />
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Fecha fin de cobertura</mat-label>
              <input matInput formControlName="endDate" type="date" />
            </mat-form-field>
          </div>
          <div class="step-actions">
            <button mat-stroked-button matStepperPrevious>
              <mat-icon>arrow_back</mat-icon> Anterior
            </button>
            <button mat-raised-button color="primary" matStepperNext [disabled]="coverageForm.invalid">
              Siguiente <mat-icon>arrow_forward</mat-icon>
            </button>
          </div>
        </form>
      </mat-step>

      <!-- Paso 3: Confirmación -->
      <mat-step label="Confirmación">
        <div class="summary">
          <h3>Resumen de la póliza</h3>
          <p><strong>Asegurado:</strong> {{ insuredForm.value.name }}</p>
          <p><strong>Documento:</strong> {{ insuredForm.value.documentId }}</p>
          <p><strong>Tipo:</strong> {{ coverageForm.value.type ?? '' | statusLabel }}</p>
          <p><strong>Monto:</strong> {{ coverageForm.value.insuredAmount | currency:'USD' }}</p>
          <p><strong>Vigencia:</strong> {{ coverageForm.value.startDate }} — {{ coverageForm.value.endDate }}</p>
        </div>
        <div class="step-actions">
          <button mat-stroked-button matStepperPrevious>
            <mat-icon>arrow_back</mat-icon> Anterior
          </button>
          <button mat-raised-button color="primary" [disabled]="submitting" (click)="submit()">
            @if (submitting) { Creando... } @else { Crear Póliza }
          </button>
        </div>
      </mat-step>
    </mat-stepper>
  `,
  styles: [`
    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
      padding: 16px 0;
    }
    .step-actions {
      display: flex;
      gap: 12px;
      margin-top: 16px;
    }
    .summary p { margin: 8px 0; }
    .summary h3 { margin-bottom: 16px; }
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
    name: ['', Validators.required],
    documentId: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
  });

  coverageForm = this.fb.group({
    type: ['Life' as PolicyType, Validators.required],
    insuredAmount: [null as number | null, [Validators.required, Validators.min(1)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
  });

  submit() {
    if (this.insuredForm.invalid || this.coverageForm.invalid) return;
    this.submitting = true;

    const iv = this.insuredForm.value;
    const cv = this.coverageForm.value;

    this.service.createPolicy({
      type: cv.type!,
      insured: {
        name: iv.name!,
        documentId: iv.documentId!,
        email: iv.email!,
        phone: iv.phone ?? '',
      },
      coveragePeriod: {
        startDate: cv.startDate!,
        endDate: cv.endDate!,
      },
      insuredAmount: cv.insuredAmount!,
    }).subscribe({
      next: () => {
        this.snackBar.open('Póliza creada exitosamente', 'Cerrar', { duration: 3000 });
        this.router.navigate(['/policies']);
      },
      error: () => { this.submitting = false; },
    });
  }
}
