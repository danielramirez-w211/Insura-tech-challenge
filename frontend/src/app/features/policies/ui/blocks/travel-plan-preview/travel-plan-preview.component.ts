import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { TravelPlanSelection } from '../../../core/models/travel-plan-selection.model';

@Component({
  selector: 'app-travel-plan-preview',
  standalone: true,
  imports: [CommonModule, MatDividerModule],
  templateUrl: './travel-plan-preview.component.html',
  styleUrl: './travel-plan-preview.component.css',
})
export class TravelPlanPreviewComponent {
  readonly calculation = input<TravelPlanSelection | null>(null);
}
