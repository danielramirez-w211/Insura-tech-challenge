import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PolicyType } from '../../../core/models/policy.model';
import { PolicyTypeCardComponent } from '../policy-type-card/policy-type-card.component';

interface PolicyTypeOption {
  type: PolicyType;
  label: string;
  description: string;
  imageSrc?: string;
  icon?: string;
}

@Component({
  selector: 'app-policy-type-selector',
  standalone: true,
  imports: [CommonModule, PolicyTypeCardComponent],
  templateUrl: './policy-type-selector.component.html',
  styleUrl: './policy-type-selector.component.css',
})
export class PolicyTypeSelectorComponent {
  selectedType = input<PolicyType | null>(null);
  typeSelected = output<PolicyType>();

  readonly options: PolicyTypeOption[] = [
    {
      type: 'Health',
      label: 'Salud',
      description: 'Cobertura médica y hospitalaria',
      imageSrc: 'Policy_images/Salud.jpg',
    },
    {
      type: 'Travel',
      label: 'Viajes',
      description: 'Asistencia y emergencias en viaje',
      imageSrc: 'Policy_images/viaje.jpg',
    },
    {
      type: 'Life',
      label: 'Vida',
      description: 'Protección de vida y beneficiarios',
      imageSrc: 'Policy_images/Vida.jpg',
    },
    {
      type: 'Vehicle',
      label: 'Vehículo',
      description: 'Protección de tu automóvil',
      imageSrc: 'Policy_images/Vehiculo.jpg',
    },
    {
      type: 'Home',
      label: 'Hogar',
      description: 'Seguro para tu vivienda',
      icon: 'home',
    },
  ];

  onCardClick(type: PolicyType): void {
    this.typeSelected.emit(type);
  }
}
