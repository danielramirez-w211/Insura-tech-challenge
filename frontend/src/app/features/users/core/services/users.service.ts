import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import {
  AdvisorSummary,
  CreateAdvisorRequest,
  CreateLeaderRequest,
  UpdateProfileRequest,
  UserDetail,
} from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/v1/users`;

  readonly loading = signal(false);

  getMe() {
    return this.http.get<UserDetail>(`${this.base}/me`);
  }

  updateProfile(body: UpdateProfileRequest) {
    return this.http.put<UserDetail>(`${this.base}/me/profile`, body);
  }

  getLeaders() {
    return this.http.get<UserDetail[]>(`${this.base}/leaders`);
  }

  createLeader(body: CreateLeaderRequest) {
    return this.http.post<UserDetail>(`${this.base}/leaders`, body);
  }

  getMyAdvisors() {
    return this.http.get<AdvisorSummary[]>(`${this.base}/my-advisors`);
  }

  createAdvisor(body: CreateAdvisorRequest) {
    return this.http.post<UserDetail>(`${this.base}/advisors`, body);
  }

  toggleStatus(id: string, isActive: boolean) {
    return this.http.patch<UserDetail>(`${this.base}/${id}/status`, { isActive });
  }
}
