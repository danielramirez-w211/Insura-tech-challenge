import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { ClaimDto } from '../../models/claim.model';

@Component({
  selector: 'app-claim-actions',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatCardModule],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>Acciones</mat-card-title>
      </mat-card-header>
      <mat-card-actions>
        @if (claim.status === 'Registered' || claim.status === 'Appealed') {
          <button mat-raised-button color="primary" (click)="approve.emit()">
            <mat-icon>check_circle</mat-icon> Aprobar
          </button>
          <button mat-raised-button color="warn" (click)="reject.emit()">
            <mat-icon>cancel</mat-icon> Rechazar
          </button>
        }
        @if (claim.status === 'Rejected') {
          <button mat-raised-button color="accent" (click)="appeal.emit()">
            <mat-icon>gavel</mat-icon> Apelar
          </button>
        }
        @if (claim.status === 'Approved') {
          <button mat-raised-button color="primary" (click)="pay.emit()">
            <mat-icon>payments</mat-icon> Marcar como Pagado
          </button>
        }
        @if (claim.status === 'Paid') {
          <span class="no-actions">Siniestro cerrado — no hay acciones disponibles.</span>
        }
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    mat-card-actions { display: flex; gap: 8px; flex-wrap: wrap; padding: 16px; }
    .no-actions { color: #666; font-size: 14px; padding: 8px 0; }
  `],
})
export class ClaimActionsComponent {
  @Input({ required: true }) claim!: ClaimDto;
  @Output() approve = new EventEmitter<void>();
  @Output() reject = new EventEmitter<void>();
  @Output() appeal = new EventEmitter<void>();
  @Output() pay = new EventEmitter<void>();
}
