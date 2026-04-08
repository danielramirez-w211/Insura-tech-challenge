import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TravelPlanCalculationDto, TripType, Continent } from '../models/policy.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TravelPlansService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/v1/travel-plans`;

  calculate(
    tripType: TripType,
    durationDays: number,
    continent?: Continent
  ): Observable<TravelPlanCalculationDto> {
    let params = new HttpParams()
      .set('tripType', tripType)
      .set('durationDays', durationDays.toString());

    if (continent) {
      params = params.set('continent', continent);
    }

    return this.http.get<TravelPlanCalculationDto>(`${this.base}/calculate`, { params });
  }
}
