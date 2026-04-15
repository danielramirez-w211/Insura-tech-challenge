import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { CityOption } from '../models/city.model';

@Injectable({ providedIn: 'root' })
export class CitiesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/cities`;

  cities = signal<CityOption[]>([]);

  loadCities(): Observable<CityOption[]> {
    return this.http.get<CityOption[]>(this.baseUrl).pipe(
      tap(cities => this.cities.set(cities))
    );
  }
}
