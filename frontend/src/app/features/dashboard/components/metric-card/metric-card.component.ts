import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-metric-card',
  standalone: true,
  imports: [MatCardModule, MatIconModule],
  template: `
    <mat-card class="metric-card">
      <mat-card-content>
        <div class="metric-icon" [style.background-color]="color + '1a'">
          <mat-icon [style.color]="color">{{ icon }}</mat-icon>
        </div>
        <div class="metric-info">
          <span class="metric-value">{{ value }}</span>
          <span class="metric-title">{{ title }}</span>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .metric-card { cursor: default; }
    mat-card-content {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 20px !important;
    }
    .metric-icon {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      mat-icon { font-size: 28px; width: 28px; height: 28px; }
    }
    .metric-info {
      display: flex;
      flex-direction: column;
    }
    .metric-value {
      font-size: 32px;
      font-weight: 700;
      line-height: 1;
    }
    .metric-title {
      font-size: 13px;
      color: #666;
      margin-top: 4px;
    }
  `],
})
export class MetricCardComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) value!: number;
  @Input({ required: true }) icon!: string;
  @Input() color = '#3f51b5';
}
