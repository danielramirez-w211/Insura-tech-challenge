import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { HealthPlan } from '../../../core/models/health-plan-selection.model';

@Component({
  selector: 'app-health-plan-selector',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './health-plan-selector.component.html',
  styleUrl: './health-plan-selector.component.css',
})
export class HealthPlanSelectorComponent {
  plans = input.required<HealthPlan[]>();
  selectedPlanId = input<string | null>(null);
  planSelected = output<HealthPlan>();

  select(plan: HealthPlan): void {
    this.planSelected.emit(plan);
  }
}
