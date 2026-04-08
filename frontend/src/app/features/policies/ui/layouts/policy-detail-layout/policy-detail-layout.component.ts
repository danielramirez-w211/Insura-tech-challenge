import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Policy } from '../../../core/models/policy.model';
import { PolicySummaryBlockComponent } from '../../blocks/policy-summary-block/policy-summary-block.component';
import { TravelDetailsBlockComponent } from '../../blocks/travel-details-block/travel-details-block.component';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-policy-detail-layout',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    PolicySummaryBlockComponent,
    TravelDetailsBlockComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './policy-detail-layout.component.html',
  styleUrl: './policy-detail-layout.component.css',
})
export class PolicyDetailLayoutComponent {
  @Input() policy: Policy | null = null;
  @Input() loading = false;
  @Input() error: string | null = null;

  @Output() activate  = new EventEmitter<string>();
  @Output() goBack    = new EventEmitter<void>();
  @Output() viewClaims = new EventEmitter<string>();
}
