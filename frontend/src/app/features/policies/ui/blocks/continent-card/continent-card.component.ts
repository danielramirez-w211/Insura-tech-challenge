import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Continent } from '../../../core/models/travel-plan-selection.model';

@Component({
  selector: 'app-continent-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './continent-card.component.html',
  styleUrl: './continent-card.component.css',
})
export class ContinentCardComponent {
  continent = input<Continent | null>(null);
  label     = input.required<string>();
  imageSrc  = input.required<string>();
  selected  = input<boolean>(false);

  cardClick = output<Continent | null>();

  onClick(): void {
    this.cardClick.emit(this.continent());
  }
}
