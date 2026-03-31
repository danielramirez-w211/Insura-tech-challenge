import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClaimsService } from '../services/claims.service';
import { ClaimDto } from '../models/claim.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ClaimStatusTimelineComponent } from '../components/claim-status-timeline/claim-status-timeline.component';
import { ClaimActionsComponent } from '../components/claim-actions/claim-actions.component';
import { AppealDialogComponent } from '../components/appeal-dialog/appeal-dialog.component';

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
  template: `
    <app-page-header
      title="Detalle de Siniestro"
      actionLabel="Volver"
      actionIcon="arrow_back"
      (action)="router.navigate(['/claims'])" />

    @if (loading) {
      <app-loading-spinner [loading]="true" />
    } @else if (claim) {
      <div class="claim-layout">
        <mat-card>
          <mat-card-header>
            <mat-card-title>{{ claim.claimNumber }}</mat-card-title>
            <mat-card-subtitle>
              <app-status-badge [status]="claim.status" />
            </mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="detail-grid">
              <div class="detail-item">
                <span class="label">Póliza</span>
                <span>{{ claim.policyNumber }}</span>
              </div>
              <div class="detail-item">
                <span class="label">Monto reclamado</span>
                <span>{{ claim.claimAmount | currency:'USD' }}</span>
              </div>
              <div class="detail-item">
                <span class="label">Descripción</span>
                <span>{{ claim.description }}</span>
              </div>
              <div class="detail-item">
                <span class="label">Fecha de registro</span>
                <span>{{ claim.createdAt | date:'dd/MM/yyyy' }}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Historial de Estados</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <app-claim-status-timeline [statusHistory]="claim.statusHistory" />
          </mat-card-content>
        </mat-card>

        <app-claim-actions
          [claim]="claim"
          (approve)="onApprove()"
          (reject)="onReject()"
          (appeal)="onAppeal()"
          (pay)="onPay()" />
      </div>
    }
  `,
  styles: [`
    .claim-layout { display: flex; flex-direction: column; gap: 16px; }
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }
    .detail-item { display: flex; flex-direction: column; gap: 4px; }
    .label { font-size: 12px; color: #666; text-transform: uppercase; }
  `],
})
export class ClaimDetailComponent implements OnInit {
  @Input() id!: string;

  service = inject(ClaimsService);
  router = inject(Router);
  dialog = inject(MatDialog);
  snackBar = inject(MatSnackBar);

  claim: ClaimDto | null = null;
  loading = false;

  ngOnInit() {
    this.loadClaim();
  }

  loadClaim() {
    this.loading = true;
    this.service.getClaim(this.id).subscribe({
      next: (c) => { this.claim = c; this.loading = false; },
      error: () => this.loading = false,
    });
  }

  onApprove() {
    if (!this.claim) return;
    this.service.approveClaim(this.claim.id, { responsibleUser: 'Operador' }).subscribe({
      next: (updated) => {
        this.claim = updated;
        this.snackBar.open('Siniestro aprobado', 'Cerrar', { duration: 3000 });
      },
    });
  }

  onReject() {
    if (!this.claim) return;
    this.service.rejectClaim(this.claim.id, { responsibleUser: 'Operador' }).subscribe({
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
      this.service.appealClaim(this.claim.id, request).subscribe({
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
