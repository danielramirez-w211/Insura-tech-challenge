import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../core/models/api-response.model';
import {
  ClaimDto,
  ClaimFilters,
} from '../../claims/models/claim.model';
import {
  PolicyDto,
  PolicyFilters,
  CreatePolicyRequest,
} from '../models/policy.model';

@Injectable({ providedIn: 'root' })
export class PoliciesService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/policies`;

  readonly policies = signal<PolicyDto[]>([]);
  readonly loading = signal(false);
  readonly totalCount = signal(0);

  getPolicies(filters: PolicyFilters) {
    const params = this.buildParams(filters);
    return this.http.get<PagedResult<PolicyDto>>(this.baseUrl, { params });
  }

  getPolicy(id: string) {
    return this.http.get<PolicyDto>(`${this.baseUrl}/${id}`);
  }

  createPolicy(request: CreatePolicyRequest) {
    return this.http.post<PolicyDto>(this.baseUrl, request);
  }

  activatePolicy(id: string) {
    return this.http.put<PolicyDto>(`${this.baseUrl}/${id}/activate`, {});
  }

  getClaimsByPolicy(policyId: string, filters: Omit<ClaimFilters, 'policyId'>) {
    const params = this.buildParams(filters);
    return this.http.get<PagedResult<ClaimDto>>(
      `${this.baseUrl}/${policyId}/claims`,
      { params }
    );
  }

  private buildParams(obj: object): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(obj as Record<string, unknown>)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
