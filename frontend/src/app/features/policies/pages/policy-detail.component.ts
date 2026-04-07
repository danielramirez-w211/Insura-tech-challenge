import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PoliciesService } from '../services/policies.service';
import { PolicyDto } from '../models/policy.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';

@Component({
  selector: 'app-policy-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    StatusLabelPipe,
  ],
  template: `
    <app-page-header
      title="Detalle de Póliza"
      actionLabel="Volver"
      actionIcon="arrow_back"
      (action)="router.navigate(['/policies'])" />

    @if (loading) {
      <app-loading-spinner [loading]="true" />
    } @else if (policy) {
      <mat-card>
        <mat-card-header>
          <mat-card-title>{{ policy.policyNumber }}</mat-card-title>
          <mat-card-subtitle>
            <app-status-badge [status]="policy.status" />
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="label">Tipo</span>
              <span>{{ policy.type | statusLabel }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Asegurado</span>
              <span>{{ policy.insured.firstName }}</span>
            </div>
          <div class="detail-item">
              <span class="label">Tipo de Documento</span>
              <span>{{ policy.insured.documentType }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Documento</span>
              <span>{{ policy.insured.documentId }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Email</span>
              <span>{{ policy.insured.email }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Monto asegurado</span>
              <span>{{ policy.insuredAmount | currency:'USD' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">Vigencia</span>
              <span>{{ policy.coveragePeriod.startDate }} — {{ policy.coveragePeriod.endDate }}</span>
            </div>
          </div>
        </mat-card-content>
        <mat-card-actions>
          @if (policy.status === 'Pending') {
            <button mat-raised-button color="primary" (click)="activate()">
              <mat-icon>play_circle</mat-icon>
              Activar Póliza
            </button>
          }
          <button mat-stroked-button (click)="router.navigate(['/claims'], { queryParams: { policyId: policy!.id } })">
            <mat-icon>report_problem</mat-icon>
            Ver Siniestros
          </button>
        </mat-card-actions>
      </mat-card>
    }
  `,
  styles: [`
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 16px;
      margin-top: 16px;
    }
    .detail-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .label {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
    }
    mat-card-actions { padding: 16px; gap: 8px; display: flex; }
  `],
})
export class PolicyDetailComponent implements OnInit {
  @Input() id!: string;

  service = inject(PoliciesService);
  router = inject(Router);
  snackBar = inject(MatSnackBar);

  policy: PolicyDto | null = null;
  loading = false;

  ngOnInit() {
    this.loading = true;
    this.service.getPolicy(this.id).subscribe({
      next: (p) => { this.policy = p; this.loading = false; },
      error: () => this.loading = false,
    });
  }

  activate() {
    if (!this.policy) return;
    this.service.activatePolicy(this.policy.id).subscribe({
      next: (updated) => {
        this.policy = updated;
        this.snackBar.open('Póliza activada exitosamente', 'Cerrar', { duration: 3000 });
      },
    });
  }
}
