import { inject, Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../core/models/api-response.model';
import { NotificationDto, NotificationFilters } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/notifications`;

  readonly notifications = signal<NotificationDto[]>([]);
  readonly loading = signal(false);
  readonly totalCount = signal(0);

  readonly failedCount = computed(
    () => this.notifications().filter((n) => n.status === 'Failed').length
  );

  getNotifications(filters: NotificationFilters) {
    const params = this.buildParams(filters);
    return this.http.get<PagedResult<NotificationDto>>(this.baseUrl, { params });
  }

  retryNotification(id: string) {
    return this.http.put<NotificationDto>(`${this.baseUrl}/${id}/retry`, {});
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
