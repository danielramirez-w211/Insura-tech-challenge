import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { HealthPlanDto, HealthPlanCalculationDto } from '../models/policy.model';

@Injectable({ providedIn: 'root' })
export class HealthPlansService {
  private http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/health-plans`;

  getPlans() {
    return this.http.get<HealthPlanDto[]>(this.baseUrl);
  }

  calculate(planId: string, birthDate: string) {
    const params = new HttpParams()
      .set('planId', planId)
      .set('birthDate', birthDate);

    return this.http.get<HealthPlanCalculationDto>(`${this.baseUrl}/calculate`, { params });
  }
}
