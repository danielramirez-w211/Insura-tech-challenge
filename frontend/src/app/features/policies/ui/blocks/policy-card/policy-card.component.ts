import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Policy } from '../../../core/models/policy.model';
import { PolicyStatusChipComponent } from '../../elements/policy-status-chip/policy-status-chip.component';
import { PolicyTypeBadgeComponent } from '../../elements/policy-type-badge/policy-type-badge.component';

@Component({
  selector: 'app-policy-card',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    PolicyStatusChipComponent,
    PolicyTypeBadgeComponent,
  ],
  templateUrl: './policy-card.component.html',
  styleUrl: './policy-card.component.css',
})
export class PolicyCardComponent {
  @Input({ required: true }) policy!: Policy;

  @Output() activate   = new EventEmitter<string>();
  @Output() viewDetail = new EventEmitter<string>();
  @Output() viewClaims = new EventEmitter<string>();

  get insuredFullName(): string {
    return `${this.policy.insured.firstName} ${this.policy.insured.lastName}`;
  }
}
