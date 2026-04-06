import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { HealthPlanCalculationDto } from '../../models/policy.model';

@Component({
  selector: 'app-health-plan-preview',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDividerModule],
  template: `
    @if (calculation()) {
      <div class="preview-card" data-testid="health-plan-preview">
        <div class="preview-header">
          <mat-icon>calculate</mat-icon>
          <span>Resumen del plan seleccionado</span>
        </div>

        <mat-divider />

        <div class="preview-grid">
          <span class="label">Plan</span>
          <span class="value">{{ calculation()!.planName }}</span>

          <span class="label">Edad del asegurado</span>
          <span class="value">{{ calculation()!.insuredAge }} años</span>

          <span class="label">Valor base</span>
          <span class="value">{{ calculation()!.baseAmount | currency:'COP':'symbol':'1.0-0' }}</span>

          <span class="label">Incremento por edad</span>
          <span class="value">
            {{ calculation()!.ageFactorPercentage }}%
            @if (calculation()!.ageFactorAmount > 0) {
              <em>(+{{ calculation()!.ageFactorAmount | currency:'COP':'symbol':'1.0-0' }})</em>
            }
          </span>
        </div>

        <mat-divider />

        <div class="final-row">
          <span class="final-label">Monto asegurado final</span>
          <span class="final-amount" data-testid="final-amount">
            {{ calculation()!.finalAmount | currency:'COP':'symbol':'1.0-0' }}
          </span>
        </div>
      </div>
    }
  `,
  styles: [`
    .preview-card {
      background: var(--mat-sys-surface-container, #f5f5f5);
      border-radius: 8px;
      padding: 16px;
      border-left: 4px solid var(--mat-sys-primary, #6750a4);
    }

    .preview-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      font-size: 13px;
      text-transform: uppercase;
      color: var(--mat-sys-primary, #6750a4);
      margin-bottom: 12px;
    }

    .preview-grid {
      display: grid;
      grid-template-columns: 180px 1fr;
      gap: 8px 16px;
      padding: 12px 0;
    }

    .label {
      color: var(--mat-sys-on-surface-variant, #666);
      font-size: 13px;
    }

    .value {
      font-weight: 500;
    }

    .final-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 12px;
    }

    .final-label {
      font-weight: 600;
    }

    .final-amount {
      font-size: 22px;
      font-weight: 700;
      color: var(--mat-sys-primary, #6750a4);
    }
  `],
})
export class HealthPlanPreviewComponent {
  calculation = input<HealthPlanCalculationDto | null>(null);
}
