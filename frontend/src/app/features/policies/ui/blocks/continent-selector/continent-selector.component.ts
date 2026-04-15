import { Component, input, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Continent, TripType } from '../../../core/models/travel-plan-selection.model';
import { ContinentCardComponent } from '../continent-card/continent-card.component';

interface ContinentOption {
  continent: Continent | null;
  label: string;
  imageSrc: string;
}

const NACIONAL_OPTION: ContinentOption = {
  continent: null,
  label: 'Colombia',
  imageSrc: 'Policy_images/Colombia.png',
};

const INTERNATIONAL_OPTIONS: ContinentOption[] = [
  { continent: 'America', label: 'América',  imageSrc: 'Policy_images/America.jpg' },
  { continent: 'Europe',  label: 'Europa',   imageSrc: 'Policy_images/Europa.jpg'  },
  { continent: 'Africa',  label: 'África',   imageSrc: 'Policy_images/Africa.jpg'  },
  { continent: 'Asia',    label: 'Asia',     imageSrc: 'Policy_images/Asia.jpg'    },
];

@Component({
  selector: 'app-continent-selector',
  standalone: true,
  imports: [CommonModule, ContinentCardComponent],
  templateUrl: './continent-selector.component.html',
  styleUrl: './continent-selector.component.css',
})
export class ContinentSelectorComponent {
  tripType          = input.required<TripType>();
  selectedContinent = input<Continent | null>(null);

  continentSelected = output<Continent | null>();

  readonly nationalOption       = NACIONAL_OPTION;
  readonly internationalOptions = INTERNATIONAL_OPTIONS;

  constructor() {
    // Auto-emit null (Colombia) whenever trip type switches to Nacional
    effect(() => {
      if (this.tripType() === 'Nacional') {
        this.continentSelected.emit(null);
      }
    });
  }

  onCardClick(continent: Continent | null): void {
    this.continentSelected.emit(continent);
  }
}
