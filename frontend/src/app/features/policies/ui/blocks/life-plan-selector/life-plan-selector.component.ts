import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { LifePlan } from '../../../core/models/life-plan-selection.model';

@Component({
  selector: 'app-life-plan-selector',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './life-plan-selector.component.html',
  styleUrl: './life-plan-selector.component.css',
})
export class LifePlanSelectorComponent {
  plans = input.required<LifePlan[]>();
  selectedPlanId = input<string | null>(null);
  planSelected = output<LifePlan>();

  select(plan: LifePlan): void {
    this.planSelected.emit(plan);
  }
}
