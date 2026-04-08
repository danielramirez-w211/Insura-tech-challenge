import { Component, Input } from '@angular/core';
import { PolicyType } from '../../../core/models/policy.model';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-policy-type-badge',
  standalone: true,
  imports: [MatChipsModule],
  templateUrl: './policy-type-badge.component.html',
  styleUrl: './policy-type-badge.component.css',
})
export class PolicyTypeBadgeComponent {
  @Input({ required: true }) type!: PolicyType;

  get label(): string {
    const labels: Record<PolicyType, string> = {
      Life: 'Vida',
      Health: 'Salud',
      Vehicle: 'Vehículo',
      Home: 'Hogar',
      Travel: 'Viajes',
    };
    return labels[this.type] ?? this.type;
  }

  get colorClass(): string {
    const classes: Record<PolicyType, string> = {
      Life: 'badge--life',
      Health: 'badge--health',
      Vehicle: 'badge--vehicle',
      Home: 'badge--home',
      Travel: 'badge--travel',
    };
    return classes[this.type] ?? '';
  }
}
