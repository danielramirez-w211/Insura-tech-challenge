import { Component, Input } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { TravelPlanSelection } from '../../../core/models/travel-plan-selection.model';

@Component({
  selector: 'app-travel-details-block',
  standalone: true,
  imports: [CurrencyPipe, MatIconModule],
  templateUrl: './travel-details-block.component.html',
  styleUrl: './travel-details-block.component.css',
})
export class TravelDetailsBlockComponent {
  @Input({ required: true }) travelPlan!: TravelPlanSelection;
}
