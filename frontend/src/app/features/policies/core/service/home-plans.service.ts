import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  HomePlanPackage,
  HomeQuotationRequest,
  HomeQuotationResult,
} from '../models/home-plan-selection.model';

@Injectable({ providedIn: 'root' })
export class HomePlansService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/home-plans`;

  getPackages(): Observable<HomePlanPackage[]> {
    return this.http.get<HomePlanPackage[]>(this.baseUrl);
  }

  calculate(request: HomeQuotationRequest): Observable<HomeQuotationResult> {
    return this.http.post<HomeQuotationResult>(`${this.baseUrl}/calculate`, request);
  }
}
