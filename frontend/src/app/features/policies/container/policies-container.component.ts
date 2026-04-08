import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PoliciesFacade } from './policies.facade';
import { PolicyType, PolicyStatus } from '../core/models/policy.model';
import { PoliciesListLayoutComponent } from '../ui/layouts/policies-list-layout/policies-list-layout.component';

/**
 * Container — único punto de coordinación entre la Facade y los componentes UI.
 * Solo inyecta PoliciesFacade, Router y MatSnackBar.
 * No contiene lógica de negocio ni transformación de datos.
 */
@Component({
  selector: 'app-policies-container',
  standalone: true,
  imports: [PoliciesListLayoutComponent],
  templateUrl: './policies-container.component.html',
})
export class PoliciesContainerComponent implements OnInit {
  private readonly facade = inject(PoliciesFacade);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  // ── Señales expuestas al template ──────────────────────────────────────────
  readonly policies = this.facade.policies;
  readonly loading = this.facade.loading;
  readonly error = this.facade.error;
  readonly total = this.facade.total;
  readonly currentFilter = this.facade.currentFilter;
  readonly currentPage = this.facade.currentPage;
  readonly currentPageSize = this.facade.currentPageSize;

  ngOnInit(): void {
    this.facade.loadPolicies();
  }

  onTypeFilterChange(filter: PolicyType | null): void {
    this.facade.setTypeFilter(filter);
    this.facade.loadPolicies();
  }

  onStatusFilterChange(status: PolicyStatus | null): void {
    this.facade.setStatusFilter(status);
    this.facade.loadPolicies();
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.facade.setPage(event.page, event.pageSize);
  }

  onActivate(id: string): void {
    this.facade.activatePolicy(id)
      .then(() => this.snackBar.open('Póliza activada exitosamente', 'Cerrar', { duration: 3000 }))
      .catch(() => {
        const msg = this.facade.error() ?? 'Error al activar póliza';
        this.snackBar.open(msg, 'Cerrar', { duration: 4000 });
      });
  }

  onViewDetail(id: string): void {
    this.router.navigate(['/policies', id]);
  }

  onViewClaims(id: string): void {
    this.router.navigate(['/claims'], { queryParams: { policyId: id } });
  }

  onNewPolicy(): void {
    this.router.navigate(['/policies/new']);
  }
}
