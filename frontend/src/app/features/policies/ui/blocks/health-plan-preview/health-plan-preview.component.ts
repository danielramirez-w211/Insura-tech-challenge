import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { HealthPlanCalculation } from '../../../core/models/health-plan-selection.model';

@Component({
  selector: 'app-health-plan-preview',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDividerModule],
  templateUrl: './health-plan-preview.component.html',
  styleUrl: './health-plan-preview.component.css',
})
export class HealthPlanPreviewComponent {
  calculation = input<HealthPlanCalculation | null>(null);
}
