import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ClaimStatusHistory } from '../../../core/models/claim.model';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';

const STATUS_ICONS: Record<string, string> = {
  Registered: 'fiber_new',
  Approved: 'check_circle',
  Rejected: 'cancel',
  Appealed: 'gavel',
  Paid: 'payments',
};

const STATUS_COLORS: Record<string, string> = {
  Registered: '#1976d2',
  Approved: '#388e3c',
  Rejected: '#d32f2f',
  Appealed: '#f57c00',
  Paid: '#388e3c',
};

@Component({
  selector: 'app-claim-status-timeline',
  standalone: true,
  imports: [CommonModule, MatIconModule, StatusLabelPipe],
  templateUrl: './claim-status-timeline.component.html',
  styleUrl: './claim-status-timeline.component.css',
})
export class ClaimStatusTimelineComponent {
  @Input({ required: true }) statusHistory: ClaimStatusHistory[] = [];

  getIcon(status: string): string {
    return STATUS_ICONS[status] ?? 'circle';
  }

  getColor(status: string): string {
    return STATUS_COLORS[status] ?? '#9e9e9e';
  }
}
