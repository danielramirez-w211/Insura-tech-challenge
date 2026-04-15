import { Component, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { HomeQuotationResult, HOME_COVERAGE_OPTIONS } from '../../../core/models/home-plan-selection.model';

@Component({
  selector: 'app-home-plan-preview',
  standalone: true,
  imports: [CurrencyPipe, MatIconModule, MatDividerModule],
  templateUrl: './home-plan-preview.component.html',
  styleUrl: './home-plan-preview.component.css',
})
export class HomePlanPreviewComponent {
  quotation = input<HomeQuotationResult | null>(null);

  readonly COVERAGE_OPTIONS = HOME_COVERAGE_OPTIONS;

  getCoverageLabel(value: string): string {
    return this.COVERAGE_OPTIONS.find(o => o.value === value)?.label ?? value;
  }
}
