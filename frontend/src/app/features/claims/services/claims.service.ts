import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../core/models/api-response.model';
import {
  ClaimDto,
  ClaimFilters,
  CreateClaimRequest,
  ClaimActionRequest,
} from '../models/claim.model';

@Injectable({ providedIn: 'root' })
export class ClaimsService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/claims`;

  readonly claims = signal<ClaimDto[]>([]);
  readonly loading = signal(false);
  readonly totalCount = signal(0);
  readonly selectedClaim = signal<ClaimDto | null>(null);

  getClaims(filters: ClaimFilters) {
    const params = this.buildParams(filters);
    return this.http.get<PagedResult<ClaimDto>>(this.baseUrl, { params });
  }

  getClaim(id: string) {
    return this.http.get<ClaimDto>(`${this.baseUrl}/${id}`);
  }

  createClaim(request: CreateClaimRequest) {
    return this.http.post<ClaimDto>(this.baseUrl, request);
  }

  approveClaim(id: string, request: ClaimActionRequest) {
    return this.http.put<ClaimDto>(`${this.baseUrl}/${id}/approve`, request);
  }

  rejectClaim(id: string, request: ClaimActionRequest) {
    return this.http.put<ClaimDto>(`${this.baseUrl}/${id}/reject`, request);
  }

  appealClaim(id: string, request: ClaimActionRequest) {
    return this.http.put<ClaimDto>(`${this.baseUrl}/${id}/appeal`, request);
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
