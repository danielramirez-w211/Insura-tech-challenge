import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { PagedResult } from '../../../../core/models/api-response.model';
import { DashboardMetrics } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class DashboardCoreService {
  private http = inject(HttpClient);

  readonly metrics = signal<DashboardMetrics | null>(null);
  readonly loading = signal(false);

  loadMetrics() {
    this.loading.set(true);
    const api = environment.apiUrl;

    return forkJoin({
      policies: this.http.get<PagedResult<unknown>>(
        `${api}/api/v1/policies`,
        { params: new HttpParams().set('status', 'Active').set('page', '1').set('pageSize', '1') }
      ),
      claims: this.http.get<PagedResult<unknown>>(
        `${api}/api/v1/claims`,
        { params: new HttpParams().set('status', 'Registered').set('page', '1').set('pageSize', '1') }
      ),
      notifications: this.http.get<PagedResult<unknown>>(
        `${api}/api/v1/notifications`,
        { params: new HttpParams().set('status', 'Failed').set('page', '1').set('pageSize', '1') }
      ),
    });
  }
}
