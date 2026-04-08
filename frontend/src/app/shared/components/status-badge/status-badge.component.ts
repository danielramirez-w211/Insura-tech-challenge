import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { StatusLabelPipe } from '../../pipes/status-label.pipe';

type StatusColor = 'primary' | 'accent' | 'warn';

const STATUS_COLORS: Record<string, StatusColor> = {
  // Policy
  Active: 'primary',
  Pending: 'accent',
  Suspended: 'warn',
  Expired: 'warn',
  Cancelled: 'warn',
  // Claim
  Registered: 'accent',
  Approved: 'primary',
  Rejected: 'warn',
  Appealed: 'accent',
  Paid: 'primary',
  // Notification
  Sent: 'primary',
  Failed: 'warn',
};

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule, MatChipsModule, StatusLabelPipe],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css',
})
export class StatusBadgeComponent {
  @Input({ required: true }) status!: string;

  get color(): StatusColor {
    return STATUS_COLORS[this.status] ?? 'accent';
  }
}
