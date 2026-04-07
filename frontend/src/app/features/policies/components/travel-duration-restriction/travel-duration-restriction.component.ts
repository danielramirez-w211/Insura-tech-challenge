import { Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-travel-duration-restriction',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="restriction-banner" data-testid="duration-exceeded-banner">
      <mat-icon class="icon">warning</mat-icon>
      <div>
        <strong>Duración máxima superada</strong>
        <p>
          Un seguro de viaje cubre máximo <strong>180 días (6 meses)</strong>.
          Para continuidad de cobertura, contrate una nueva póliza a partir del día 181.
        </p>
      </div>
    </div>
  `,
  styles: [`
    .restriction-banner {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      background: #fff3cd;
      border: 1px solid #ffc107;
      border-radius: 8px;
      padding: 12px 16px;
    }
    .icon { color: #856404; margin-top: 2px; }
    strong { color: #856404; display: block; margin-bottom: 4px; }
    p { margin: 0; font-size: 13px; color: #664d03; }
  `]
})
export class TravelDurationRestrictionComponent {}
