import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { VehiclePlan, VehicleQuotationResult } from '../models/vehicle-plan-selection.model';

@Injectable({ providedIn: 'root' })
export class VehiclePlansService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/vehicle-plans`;

  getPlans() {
    return this.http.get<VehiclePlan[]>(this.baseUrl);
  }

  calculate(commercialValue: number, vehicleYear: number, brand: string) {
    const params = new HttpParams()
      .set('commercialValue', commercialValue.toString())
      .set('vehicleYear', vehicleYear.toString())
      .set('brand', brand);
    return this.http.get<VehicleQuotationResult>(`${this.baseUrl}/calculate`, { params });
  }
}
