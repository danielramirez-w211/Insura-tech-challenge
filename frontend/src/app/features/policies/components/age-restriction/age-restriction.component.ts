import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-age-restriction',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="restriction-banner" data-testid="age-restriction-banner">
      <mat-icon class="restriction-icon">warning</mat-icon>
      <div class="restriction-content">
        <p class="restriction-title">Evaluación especial requerida</p>
        <p class="restriction-message">
          El asegurado supera los 73 años. Para continuar, debe completar el
          <strong>Formulario de Preexistencias y Enfermedades</strong>.
          Contacte al área de suscripción especial para gestionar este caso.
        </p>
      </div>
    </div>
  `,
  styles: [`
    .restriction-banner {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      background: #fff3e0;
      border: 1px solid #ff9800;
      border-radius: 8px;
      padding: 16px;
      margin: 8px 0;
    }

    .restriction-icon {
      color: #f57c00;
      font-size: 28px;
      width: 28px;
      height: 28px;
      flex-shrink: 0;
    }

    .restriction-title {
      font-weight: 600;
      color: #e65100;
      margin: 0 0 6px;
    }

    .restriction-message {
      font-size: 13px;
      color: #5d4037;
      margin: 0;
      line-height: 1.5;
    }
  `],
})
export class AgeRestrictionComponent {}
