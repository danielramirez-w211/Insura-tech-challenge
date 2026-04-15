import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { LifePlanCalculation } from '../../../core/models/life-plan-selection.model';

@Component({
  selector: 'app-life-plan-preview',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatDividerModule],
  templateUrl: './life-plan-preview.component.html',
  styleUrl: './life-plan-preview.component.css',
})
export class LifePlanPreviewComponent {
  calculation = input<LifePlanCalculation | null>(null);
}
