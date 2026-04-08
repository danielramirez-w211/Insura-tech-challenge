import { computed, inject, Injectable, signal } from '@angular/core';
import { Policy, PolicyFilters, PolicyStatus, PolicyType } from '../models/policy.model';
import { PoliciesCoreService } from '../service/policies.service';
import { CreatePolicyRequest } from '../resource/policy-request.resource';
import { PolicyResponse } from '../resource/policy-response.resource';

interface PoliciesStateModel {
  items: Policy[];
  total: number;
  loading: boolean;
  error: string | null;
  selectedId: string | null;
  filter: PolicyType | null;
  statusFilter: PolicyStatus | null;
  page: number;
  pageSize: number;
}

const INITIAL_STATE: PoliciesStateModel = {
  items: [],
  total: 0,
  loading: false,
  error: null,
  selectedId: null,
  filter: null,
  statusFilter: null,
  page: 1,
  pageSize: 10,
};

function mapResponseToPolicy(r: PolicyResponse): Policy {
  return r as unknown as Policy;
}

/**
 * Signal-based state service — reemplaza NgRx Store para el módulo de pólizas.
 * Toda la lógica de estado vive aquí; la Facade es la única interfaz pública hacia afuera.
 */
@Injectable({ providedIn: 'root' })
export class PoliciesState {
  private readonly svc = inject(PoliciesCoreService);
  private readonly _state = signal<PoliciesStateModel>(INITIAL_STATE);

  // ── Selectores (equivalentes a NgRx selectors) ─────────────────────────────

  readonly policies = computed(() => {
    const { items, filter } = this._state();
    return filter ? items.filter(p => p.type === filter) : items;
  });

  readonly loading = computed(() => this._state().loading);
  readonly error = computed(() => this._state().error);
  readonly total = computed(() => this._state().total);
  readonly currentFilter = computed(() => this._state().filter);
  readonly currentPage = computed(() => this._state().page);
  readonly currentPageSize = computed(() => this._state().pageSize);

  readonly selectedPolicy = computed(() => {
    const { items, selectedId } = this._state();
    return selectedId ? (items.find(p => p.id === selectedId) ?? null) : null;
  });

  // ── Acciones (equivalentes a NgRx actions + effects) ───────────────────────

  loadPolicies(overrides?: Partial<PolicyFilters>): void {
    const { page, pageSize, filter, statusFilter } = this._state();
    const filters: PolicyFilters = {
      page,
      pageSize,
      ...(filter ? { type: filter } : {}),
      ...(statusFilter ? { status: statusFilter } : {}),
      ...overrides,
    };

    this._state.update(s => ({ ...s, loading: true, error: null }));

    this.svc.getAll(filters).subscribe({
      next: result => this._state.update(s => ({
        ...s,
        items: result.items.map(mapResponseToPolicy),
        total: result.totalCount,
        loading: false,
      })),
      error: err => this._state.update(s => ({
        ...s,
        loading: false,
        error: err?.error?.detail ?? 'Error al cargar pólizas',
      })),
    });
  }

  createPolicy(request: CreatePolicyRequest): Promise<Policy> {
    this._state.update(s => ({ ...s, loading: true, error: null }));

    return new Promise((resolve, reject) => {
      this.svc.create(request).subscribe({
        next: response => {
          const policy = mapResponseToPolicy(response);
          this._state.update(s => ({
            ...s,
            items: [policy, ...s.items],
            total: s.total + 1,
            loading: false,
          }));
          resolve(policy);
        },
        error: err => {
          this._state.update(s => ({
            ...s,
            loading: false,
            error: err?.error?.detail ?? 'Error al crear póliza',
          }));
          reject(err);
        },
      });
    });
  }

  activatePolicy(id: string): Promise<void> {
    this._state.update(s => ({ ...s, loading: true, error: null }));

    return new Promise((resolve, reject) => {
      this.svc.activate(id).subscribe({
        next: updated => {
          const policy = mapResponseToPolicy(updated);
          this._state.update(s => ({
            ...s,
            items: s.items.map(p => (p.id === id ? policy : p)),
            loading: false,
          }));
          resolve();
        },
        error: err => {
          this._state.update(s => ({
            ...s,
            loading: false,
            error: err?.error?.detail ?? 'Error al activar póliza',
          }));
          reject(err);
        },
      });
    });
  }

  selectPolicy(id: string): void {
    this._state.update(s => ({ ...s, selectedId: id }));
  }

  setTypeFilter(filter: PolicyType | null): void {
    this._state.update(s => ({ ...s, filter, page: 1 }));
  }

  setStatusFilter(status: PolicyStatus | null): void {
    this._state.update(s => ({ ...s, statusFilter: status, page: 1 }));
  }

  setPage(page: number, pageSize?: number): void {
    this._state.update(s => ({ ...s, page, pageSize: pageSize ?? s.pageSize }));
    this.loadPolicies();
  }

  clearError(): void {
    this._state.update(s => ({ ...s, error: null }));
  }
}
