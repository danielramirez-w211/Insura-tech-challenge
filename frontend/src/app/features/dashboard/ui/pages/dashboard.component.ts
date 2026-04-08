import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { DashboardCoreService } from '../../core/service/dashboard.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { MetricCardComponent } from '../blocks/metric-card/metric-card.component';

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
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
  service = inject(DashboardCoreService);
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
