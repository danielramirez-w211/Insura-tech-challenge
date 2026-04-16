import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { ClientSummary } from '../models/client.model';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/policies`;

  getMyClients() {
    return this.http.get<ClientSummary[]>(`${this.baseUrl}/my-clients`);
  }
}