import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { HealthPlanDto } from '../../models/policy.model';

@Component({
  selector: 'app-health-plan-selector',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="plans-grid">
      @for (plan of plans(); track plan.planId) {
        <mat-card
          class="plan-card"
          [class.selected]="selectedPlanId() === plan.planId"
          (click)="select(plan)"
          role="button"
          [attr.aria-pressed]="selectedPlanId() === plan.planId"
          [attr.data-testid]="'plan-' + plan.planId">

          <mat-card-content>
            <div class="plan-icon">
              <mat-icon>health_and_safety</mat-icon>
            </div>
            <h3 class="plan-name">{{ plan.planName }}</h3>
            <p class="plan-amount">{{ plan.baseAmount | currency:'COP':'symbol':'1.0-0' }}</p>
            <p class="plan-label">Valor base</p>

            @if (selectedPlanId() === plan.planId) {
              <div class="selected-badge">
                <mat-icon>check_circle</mat-icon>
              </div>
            }
          </mat-card-content>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .plans-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
      padding: 8px 0;
    }

    .plan-card {
      cursor: pointer;
      transition: all 0.2s ease;
      border: 2px solid transparent;
      position: relative;
      text-align: center;
    }

    .plan-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }

    .plan-card.selected {
      border-color: var(--mat-sys-primary, #6750a4);
      background: var(--mat-sys-primary-container, #eaddff);
    }

    .plan-icon {
      font-size: 36px;
      margin-bottom: 8px;
      color: var(--mat-sys-primary, #6750a4);
    }

    .plan-icon mat-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
    }

    .plan-name {
      font-size: 14px;
      font-weight: 600;
      margin: 0 0 8px;
    }

    .plan-amount {
      font-size: 18px;
      font-weight: 700;
      color: var(--mat-sys-primary, #6750a4);
      margin: 0 0 4px;
    }

    .plan-label {
      font-size: 11px;
      color: var(--mat-sys-on-surface-variant, #666);
      margin: 0;
    }

    .selected-badge {
      position: absolute;
      top: 8px;
      right: 8px;
      color: var(--mat-sys-primary, #6750a4);
    }
  `],
})
export class HealthPlanSelectorComponent {
  plans = input.required<HealthPlanDto[]>();
  selectedPlanId = input<string | null>(null);
  planSelected = output<HealthPlanDto>();

  select(plan: HealthPlanDto): void {
    this.planSelected.emit(plan);
  }
}
