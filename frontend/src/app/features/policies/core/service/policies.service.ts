import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { PolicyFilters } from '../models/policy.model';
import { CreatePolicyRequest } from '../resource/policy-request.resource';
import { PagedPolicyResponse, PolicyResponse } from '../resource/policy-response.resource';

/**
 * HTTP-only service — sin signals, sin estado.
 * Responsabilidad exclusiva: comunicación con la API REST de pólizas.
 * La gestión de estado es responsabilidad de PoliciesState.
 */
@Injectable({ providedIn: 'root' })
export class PoliciesCoreService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/policies`;

  getAll(filters: PolicyFilters) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters as unknown as Record<string, unknown>)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedPolicyResponse>(this.baseUrl, { params });
  }

  getById(id: string) {
    return this.http.get<PolicyResponse>(`${this.baseUrl}/${id}`);
  }

  create(request: CreatePolicyRequest) {
    return this.http.post<PolicyResponse>(this.baseUrl, request);
  }

  activate(id: string) {
    return this.http.put<PolicyResponse>(`${this.baseUrl}/${id}/activate`, {});
  }

  cancel(id: string) {
    return this.http.put<PolicyResponse>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
