import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { DashboardService } from '../services/dashboard.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { MetricCardComponent } from '../components/metric-card/metric-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    MetricCardComponent,
  ],
  template: `
    <h1 class="page-title">Dashboard</h1>

    @if (service.loading()) {
      <app-loading-spinner [loading]="true" />
    } @else if (hasError) {
      <app-empty-state
        message="No se pudo cargar la información"
        icon="cloud_off"
        [showRetry]="true"
        (retry)="load()" />
    } @else {
      <div class="metrics-grid">
        <app-metric-card
          title="Pólizas Activas"
          [value]="metrics.activePolicies"
          icon="policy"
          color="#3f51b5" />
        <app-metric-card
          title="Siniestros Pendientes"
          [value]="metrics.pendingClaims"
          icon="report_problem"
          color="#ff9800" />
        <app-metric-card
          title="Notificaciones Fallidas"
          [value]="metrics.failedNotifications"
          icon="notifications_off"
          color="#f44336" />
      </div>

      <div class="shortcuts">
        <h2>Accesos rápidos</h2>
        <div class="shortcut-buttons">
          <button mat-raised-button color="primary" (click)="router.navigate(['/policies'])">
            <mat-icon>policy</mat-icon>
            Ver Pólizas
          </button>
          <button mat-raised-button (click)="router.navigate(['/claims'])">
            <mat-icon>report_problem</mat-icon>
            Ver Siniestros
          </button>
          <button mat-raised-button (click)="router.navigate(['/notifications'])">
            <mat-icon>notifications</mat-icon>
            Ver Notificaciones
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-title { font-size: 24px; font-weight: 500; margin-bottom: 24px; }
    .metrics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
      margin-bottom: 32px;
    }
    .shortcuts h2 { font-size: 18px; font-weight: 500; margin-bottom: 16px; }
    .shortcut-buttons { display: flex; gap: 12px; flex-wrap: wrap; }
  `],
})
export class DashboardComponent implements OnInit {
  service = inject(DashboardService);
  router = inject(Router);

  metrics = { activePolicies: 0, pendingClaims: 0, failedNotifications: 0 };
  hasError = false;

  ngOnInit() {
    this.load();
  }

  load() {
    this.hasError = false;
    this.service.loading.set(true);
    this.service.loadMetrics().subscribe({
      next: (result) => {
        this.metrics = {
          activePolicies: result.policies.totalCount,
          pendingClaims: result.claims.totalCount,
          failedNotifications: result.notifications.totalCount,
        };
        this.service.metrics.set(this.metrics);
        this.service.loading.set(false);
      },
      error: () => {
        this.hasError = true;
        this.service.loading.set(false);
      },
    });
  }
}
