import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ClaimStatusHistoryDto } from '../../models/claim.model';
import { StatusLabelPipe } from '../../../../shared/pipes/status-label.pipe';

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
  template: `
    <div class="timeline">
      @for (entry of statusHistory; track entry.changedAt) {
        <div class="timeline-item">
          <div class="timeline-icon" [style.background-color]="getColor(entry.status)">
            <mat-icon>{{ getIcon(entry.status) }}</mat-icon>
          </div>
          <div class="timeline-content">
            <span class="status-label">{{ entry.status | statusLabel }}</span>
            <span class="date">{{ entry.changedAt | date:'dd/MM/yyyy HH:mm' }}</span>
            <span class="user">{{ entry.responsibleUser }}</span>
            @if (entry.observations) {
              <span class="observations">{{ entry.observations }}</span>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .timeline { display: flex; flex-direction: column; gap: 0; padding: 8px 0; }
    .timeline-item {
      display: flex;
      align-items: flex-start;
      gap: 16px;
      position: relative;
      padding-bottom: 24px;
    }
    .timeline-item:not(:last-child)::before {
      content: '';
      position: absolute;
      left: 19px;
      top: 40px;
      bottom: 0;
      width: 2px;
      background: #e0e0e0;
    }
    .timeline-icon {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      mat-icon { color: white; font-size: 20px; width: 20px; height: 20px; }
    }
    .timeline-content {
      display: flex;
      flex-direction: column;
      gap: 2px;
      padding-top: 8px;
    }
    .status-label { font-weight: 600; font-size: 14px; }
    .date { font-size: 12px; color: #666; }
    .user { font-size: 12px; color: #999; }
    .observations { font-size: 13px; color: #444; margin-top: 4px; font-style: italic; }
  `],
})
export class ClaimStatusTimelineComponent {
  @Input({ required: true }) statusHistory: ClaimStatusHistoryDto[] = [];

  getIcon(status: string): string {
    return STATUS_ICONS[status] ?? 'circle';
  }

  getColor(status: string): string {
    return STATUS_COLORS[status] ?? '#9e9e9e';
  }
}
