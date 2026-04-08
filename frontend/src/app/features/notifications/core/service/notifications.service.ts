import { inject, Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { PagedResult } from '../../../../core/models/api-response.model';
import { Notification, NotificationFilters } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationsCoreService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/notifications`;

  readonly notifications = signal<Notification[]>([]);
  readonly loading = signal(false);
  readonly totalCount = signal(0);

  readonly failedCount = computed(
    () => this.notifications().filter((n) => n.status === 'Failed').length
  );

  getNotifications(filters: NotificationFilters) {
    return this.http.get<PagedResult<Notification>>(this.baseUrl, { params: this.buildParams(filters) });
  }

  retryNotification(id: string) {
    return this.http.put<Notification>(`${this.baseUrl}/${id}/retry`, {});
  }

  private buildParams(filters: NotificationFilters): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters as unknown as Record<string, unknown>)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
