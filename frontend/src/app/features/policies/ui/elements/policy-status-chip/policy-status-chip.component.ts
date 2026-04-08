import { Component, Input } from '@angular/core';
import { PolicyStatus } from '../../../core/models/policy.model';

@Component({
  selector: 'app-policy-status-chip',
  standalone: true,
  imports: [],
  templateUrl: './policy-status-chip.component.html',
  styleUrl: './policy-status-chip.component.css',
})
export class PolicyStatusChipComponent {
  @Input({ required: true }) status!: PolicyStatus;

  get label(): string {
    const labels: Record<PolicyStatus, string> = {
      Pending: 'Pendiente',
      Active: 'Activa',
      Suspended: 'Suspendida',
      Expired: 'Vencida',
      Cancelled: 'Cancelada',
    };
    return labels[this.status] ?? this.status;
  }

  get colorClass(): string {
    const classes: Record<PolicyStatus, string> = {
      Pending: 'chip--pending',
      Active: 'chip--active',
      Suspended: 'chip--suspended',
      Expired: 'chip--expired',
      Cancelled: 'chip--cancelled',
    };
    return classes[this.status] ?? '';
  }
}
