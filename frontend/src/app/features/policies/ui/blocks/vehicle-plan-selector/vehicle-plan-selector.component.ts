import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { VehiclePlanOption } from '../../../core/models/vehicle-plan-selection.model';

@Component({
  selector: 'app-vehicle-plan-selector',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './vehicle-plan-selector.component.html',
  styleUrl: './vehicle-plan-selector.component.css',
})
export class VehiclePlanSelectorComponent {
  plans          = input.required<VehiclePlanOption[]>();
  selectedPlanId = input<string | null>(null);
  planSelected   = output<VehiclePlanOption>();

  select(plan: VehiclePlanOption): void {
    this.planSelected.emit(plan);
  }
}
