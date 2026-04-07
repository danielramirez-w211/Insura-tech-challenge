import { Component, input } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { TravelPlanCalculationDto } from '../../models/policy.model';

@Component({
  selector: 'app-travel-plan-preview',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, MatDividerModule],
  template: `
    @if (calculation()) {
      <div class="preview-card" data-testid="travel-plan-preview">
        <p class="preview-title">Resumen del cálculo</p>
        <mat-divider />

        <div class="preview-grid">
          <span class="label">Tipo de viaje</span>
          <span>{{ calculation()!.tripType }}</span>

          @if (calculation()!.continent) {
            <span class="label">Continente</span>
            <span>{{ calculation()!.continent }}</span>
          }

          <span class="label">Días de cobertura</span>
          <span>{{ calculation()!.durationDays }} días</span>

          @if (calculation()!.basePriceUsd) {
            <span class="label">Precio base (USD)</span>
            <span>{{ calculation()!.basePriceUsd | currency:'USD':'symbol':'1.2-2' }}</span>
          }

          <span class="label">Precio base (COP)</span>
          <span>{{ calculation()!.basePriceCop | currency:'COP':'symbol':'1.0-0' }}</span>

          <span class="label">Incremento diario</span>
          <span>{{ calculation()!.dailyIncrementCop | currency:'COP':'symbol':'1.0-0' }}</span>

          @if (calculation()!.trmUsed) {
            <span class="label">TRM aplicada</span>
            <span data-testid="trm-value">{{ calculation()!.trmUsed | currency:'COP':'symbol':'1.2-2' }}</span>

            <span class="label">Fecha TRM</span>
            <span>{{ calculation()!.trmDate }}</span>
          }

          <mat-divider class="full-divider" />

          <span class="label total-label">Total a pagar (COP)</span>
          <span class="total-value" data-testid="final-amount-cop">
            {{ calculation()!.totalPriceCop | currency:'COP':'symbol':'1.0-0' }}
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
      margin-top: 16px;
    }
    .preview-title {
      font-weight: 600;
      font-size: 13px;
      text-transform: uppercase;
      letter-spacing: .5px;
      color: var(--mat-sys-primary, #6750a4);
      margin: 0 0 8px;
    }
    .preview-grid {
      display: grid;
      grid-template-columns: 180px 1fr;
      gap: 8px 16px;
      margin-top: 12px;
      align-items: center;
    }
    .label { color: var(--mat-sys-on-surface-variant, #666); font-size: 13px; }
    .full-divider { grid-column: 1 / -1; margin: 4px 0; }
    .total-label { font-weight: 600; }
    .total-value { font-weight: 700; font-size: 18px; color: var(--mat-sys-primary, #6750a4); }
  `]
})
export class TravelPlanPreviewComponent {
  readonly calculation = input<TravelPlanCalculationDto | null>(null);
}
