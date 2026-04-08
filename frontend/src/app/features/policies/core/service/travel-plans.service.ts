import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { TravelPlanSelection, TripType, Continent } from '../models/travel-plan-selection.model';

@Injectable({ providedIn: 'root' })
export class TravelPlansService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/v1/travel-plans`;

  calculate(tripType: TripType, durationDays: number, continent?: Continent) {
    let params = new HttpParams()
      .set('tripType', tripType)
      .set('durationDays', durationDays.toString());

    if (continent) {
      params = params.set('continent', continent);
    }

    return this.http.get<TravelPlanSelection>(`${this.base}/calculate`, { params });
  }
}
