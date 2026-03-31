import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-appeal-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
  ],
  template: `
    <h2 mat-dialog-title>Registrar Apelación</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Usuario responsable</mat-label>
          <input matInput formControlName="responsibleUser" />
          @if (form.get('responsibleUser')?.hasError('required')) {
            <mat-error>Campo obligatorio</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Observaciones</mat-label>
          <textarea matInput formControlName="observations" rows="4"
            placeholder="Detalle las razones de la apelación..."></textarea>
          @if (form.get('observations')?.hasError('required')) {
            <mat-error>Campo obligatorio</mat-error>
          }
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-stroked-button [mat-dialog-close]="null">Cancelar</button>
      <button mat-raised-button color="primary"
        [disabled]="form.invalid"
        [mat-dialog-close]="form.value">
        Confirmar Apelación
      </button>
    </mat-dialog-actions>
  `,
  styles: [`.full-width { width: 100%; min-width: 360px; } mat-dialog-content { padding-top: 16px !important; }`],
})
export class AppealDialogComponent {
  dialogRef = inject(MatDialogRef<AppealDialogComponent>);
  fb = inject(FormBuilder);

  form = this.fb.group({
    responsibleUser: ['', Validators.required],
    observations: ['', Validators.required],
  });
}
