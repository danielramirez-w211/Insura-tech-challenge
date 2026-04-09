import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { PolicyType } from '../../../core/models/policy.model';

@Component({
  selector: 'app-policy-type-card',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './policy-type-card.component.html',
  styleUrl: './policy-type-card.component.css',
})
export class PolicyTypeCardComponent {
  type        = input.required<PolicyType>();
  selected    = input<boolean>(false);
  imageSrc    = input<string | undefined>(undefined);
  icon        = input<string | undefined>(undefined);
  label       = input.required<string>();
  description = input.required<string>();

  cardClick = output<PolicyType>();

  onClick(): void {
    this.cardClick.emit(this.type());
  }
}
