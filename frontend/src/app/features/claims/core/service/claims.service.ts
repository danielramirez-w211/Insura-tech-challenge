import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { PagedResult } from '../../../../core/models/api-response.model';
import {
  Claim,
  ClaimFilters,
  CreateClaimRequest,
  ClaimActionRequest,
} from '../models/claim.model';

@Injectable({ providedIn: 'root' })
export class ClaimsCoreService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/claims`;

  readonly claims = signal<Claim[]>([]);
  readonly loading = signal(false);
  readonly totalCount = signal(0);
  readonly selectedClaim = signal<Claim | null>(null);

  getAll(filters: ClaimFilters) {
    return this.http.get<PagedResult<Claim>>(this.baseUrl, { params: this.buildParams(filters) });
  }

  getById(id: string) {
    return this.http.get<Claim>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateClaimRequest) {
    return this.http.post<Claim>(this.baseUrl, request);
  }

  approve(id: string, request: ClaimActionRequest) {
    return this.http.put<Claim>(`${this.baseUrl}/${id}/approve`, request);
  }

  reject(id: string, request: ClaimActionRequest) {
    return this.http.put<Claim>(`${this.baseUrl}/${id}/reject`, request);
  }

  appeal(id: string, request: ClaimActionRequest) {
    return this.http.put<Claim>(`${this.baseUrl}/${id}/appeal`, request);
  }

  private buildParams(filters: ClaimFilters): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters as unknown as Record<string, unknown>)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
