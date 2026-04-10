import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { LifePlan, LifePlanCalculation } from '../models/life-plan-selection.model';

@Injectable({ providedIn: 'root' })
export class LifePlansService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/life-plans`;

  getPlans() {
    return this.http.get<LifePlan[]>(this.baseUrl);
  }

  calculate(planId: string, birthDate: string) {
    const params = new HttpParams()
      .set('planId', planId)
      .set('birthDate', birthDate);
    return this.http.get<LifePlanCalculation>(`${this.baseUrl}/calculate`, { params });
  }
}
