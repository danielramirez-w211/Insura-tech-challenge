import { inject, Injectable } from '@angular/core';
import { PoliciesState } from '../core/state/policies.state';
import { Policy, PolicyFilters, PolicyStatus, PolicyType } from '../core/models/policy.model';
import { CreatePolicyRequest } from '../core/resource/policy-request.resource';

/**
 * Facade — única interfaz pública entre el Container y el estado.
 * El Container inyecta solo esta clase; nunca accede a PoliciesState directamente.
 */
@Injectable({ providedIn: 'root' })
export class PoliciesFacade {
  private readonly state = inject(PoliciesState);

  // ── Señales expuestas (readonly) ───────────────────────────────────────────
  readonly policies = this.state.policies;
  readonly loading = this.state.loading;
  readonly error = this.state.error;
  readonly total = this.state.total;
  readonly selectedPolicy = this.state.selectedPolicy;
  readonly currentFilter = this.state.currentFilter;
  readonly currentPage = this.state.currentPage;
  readonly currentPageSize = this.state.currentPageSize;

  // ── Acciones delegadas ─────────────────────────────────────────────────────
  loadPolicies(overrides?: Partial<PolicyFilters>): void {
    this.state.loadPolicies(overrides);
  }

  createPolicy(request: CreatePolicyRequest): Promise<Policy> {
    return this.state.createPolicy(request);
  }

  activatePolicy(id: string): Promise<void> {
    return this.state.activatePolicy(id);
  }

  cancelPolicy(id: string): Promise<void> {
    return this.state.cancelPolicy(id);
  }

  selectPolicy(id: string): void {
    this.state.selectPolicy(id);
  }

  setTypeFilter(filter: PolicyType | null): void {
    this.state.setTypeFilter(filter);
  }

  setStatusFilter(status: PolicyStatus | null): void {
    this.state.setStatusFilter(status);
  }

  setPage(page: number, pageSize?: number): void {
    this.state.setPage(page, pageSize);
  }

  clearError(): void {
    this.state.clearError();
  }
}
