import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
  ],
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>
            <mat-icon>policy</mat-icon>
            InsuraTech
          </mat-card-title>
          <mat-card-subtitle>Sistema de Gestión de Seguros</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" autocomplete="email" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Contraseña</mat-label>
              <input matInput formControlName="password" type="password" autocomplete="current-password" />
            </mat-form-field>
          </form>
        </mat-card-content>
        <mat-card-actions>
          <button mat-raised-button color="primary" class="full-width" [disabled]="form.invalid">
            Iniciar Sesión
          </button>
        </mat-card-actions>
        <mat-card-footer>
          <p class="hint">Autenticación disponible en SPEC-003</p>
        </mat-card-footer>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 70vh;
    }
    .login-card { width: 100%; max-width: 420px; padding: 16px; }
    mat-card-header { margin-bottom: 16px; }
    mat-card-title { display: flex; align-items: center; gap: 8px; font-size: 22px; }
    .full-width { width: 100%; }
    mat-card-actions { padding: 0 16px 16px; }
    .hint { text-align: center; color: #999; font-size: 12px; padding: 8px; }
  `],
})
export class LoginComponent {
  private fb = inject(FormBuilder);

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });
}
