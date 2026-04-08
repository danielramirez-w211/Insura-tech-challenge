import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClaimsCoreService } from '../../../core/service/claims.service';
import { Claim } from '../../../core/models/claim.model';
import { StatusBadgeComponent } from '../../../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { ClaimStatusTimelineComponent } from '../../blocks/claim-status-timeline/claim-status-timeline.component';
import { ClaimActionsComponent } from '../../blocks/claim-actions/claim-actions.component';
import { AppealDialogComponent } from '../../blocks/appeal-dialog/appeal-dialog.component';

@Component({
  selector: 'app-claim-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    ClaimStatusTimelineComponent,
    ClaimActionsComponent,
  ],
  templateUrl: './claim-detail.component.html',
  styleUrl: './claim-detail.component.css',
})
export class ClaimDetailComponent implements OnInit {
  @Input() id!: string;

  service = inject(ClaimsCoreService);
  router = inject(Router);
  dialog = inject(MatDialog);
  snackBar = inject(MatSnackBar);

  claim: Claim | null = null;
  loading = false;

  ngOnInit() {
    this.loadClaim();
  }

  loadClaim() {
    this.loading = true;
    this.service.getById(this.id).subscribe({
      next: (c) => { this.claim = c; this.loading = false; },
      error: () => this.loading = false,
    });
  }

  onApprove() {
    if (!this.claim) return;
    this.service.approve(this.claim.id, { responsibleUser: 'Operador' }).subscribe({
      next: (updated) => {
        this.claim = updated;
        this.snackBar.open('Siniestro aprobado', 'Cerrar', { duration: 3000 });
      },
    });
  }

  onReject() {
    if (!this.claim) return;
    this.service.reject(this.claim.id, { responsibleUser: 'Operador' }).subscribe({
      next: (updated) => {
        this.claim = updated;
        this.snackBar.open('Siniestro rechazado', 'Cerrar', { duration: 3000 });
      },
    });
  }

  onAppeal() {
    const ref = this.dialog.open(AppealDialogComponent);
    ref.afterClosed().subscribe((request) => {
      if (!request || !this.claim) return;
      this.service.appeal(this.claim.id, request).subscribe({
        next: (updated) => {
          this.claim = updated;
          this.snackBar.open('Apelación registrada', 'Cerrar', { duration: 3000 });
        },
      });
    });
  }

  onPay() {
    if (!this.claim) return;
    this.snackBar.open('Funcionalidad de pago próximamente', 'Cerrar', { duration: 3000 });
  }
}
