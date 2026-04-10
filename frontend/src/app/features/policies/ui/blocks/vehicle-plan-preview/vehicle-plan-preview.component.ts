import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { VehiclePlanOption, VehicleQuotationResult } from '../../../core/models/vehicle-plan-selection.model';

@Component({
  selector: 'app-vehicle-plan-preview',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDividerModule],
  templateUrl: './vehicle-plan-preview.component.html',
  styleUrl: './vehicle-plan-preview.component.css',
})
export class VehiclePlanPreviewComponent {
  quotation    = input<VehicleQuotationResult | null>(null);
  selectedPlan = input<VehiclePlanOption | null>(null);
}
