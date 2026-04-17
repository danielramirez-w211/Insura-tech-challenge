import { Component, input, output } from '@angular/core';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

export interface PolicySearchResult {
  id:           string;
  insuredName:  string;
  documentType: string;
  documentId:   string;
  policyType:   string;
  status:       string;
}

@Component({
  selector: 'app-search-result-overlay',
  standalone: true,
  imports: [MatListModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './search-result-overlay.component.html',
  styleUrl: './search-result-overlay.component.css',
})
export class SearchResultOverlayComponent {
  readonly results     = input.required<PolicySearchResult[]>();
  readonly loading     = input<boolean>(false);
  readonly resultClick = output<PolicySearchResult>();
}