import { Component, Input } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { Policy } from '../../../core/models/policy.model';
import { PolicyStatusChipComponent } from '../../elements/policy-status-chip/policy-status-chip.component';
import { PolicyTypeBadgeComponent } from '../../elements/policy-type-badge/policy-type-badge.component';

@Component({
  selector: 'app-policy-summary-block',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, MatDividerModule, PolicyStatusChipComponent, PolicyTypeBadgeComponent],
  templateUrl: './policy-summary-block.component.html',
  styleUrl: './policy-summary-block.component.css',
})
export class PolicySummaryBlockComponent {
  @Input({ required: true }) policy!: Policy;

  get insuredFullName(): string {
    return `${this.policy.insured.firstName} ${this.policy.insured.lastName}`;
  }
}
