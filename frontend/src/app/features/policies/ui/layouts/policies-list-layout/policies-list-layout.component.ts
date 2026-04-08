import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Policy, PolicyStatus, PolicyType } from '../../../core/models/policy.model';
import { PolicyFilterBarComponent } from '../../elements/policy-filter-bar/policy-filter-bar.component';
import { PolicyStatusChipComponent } from '../../elements/policy-status-chip/policy-status-chip.component';
import { PolicyTypeBadgeComponent } from '../../elements/policy-type-badge/policy-type-badge.component';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-policies-list-layout',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatProgressBarModule,
    PolicyFilterBarComponent,
    PolicyStatusChipComponent,
    PolicyTypeBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
  ],
  templateUrl: './policies-list-layout.component.html',
  styleUrl: './policies-list-layout.component.css',
})
export class PoliciesListLayoutComponent {
  @Input() set policies(value: Policy[]) {
    this.dataSource.data = value;
  }
  @Input() loading = false;
  @Input() error: string | null = null;
  @Input() total = 0;
  @Input() currentFilter: PolicyType | null = null;
  @Input() currentPage = 1;
  @Input() currentPageSize = 10;

  @Output() typeFilterChange   = new EventEmitter<PolicyType | null>();
  @Output() statusFilterChange = new EventEmitter<PolicyStatus | null>();
  @Output() pageChange         = new EventEmitter<{ page: number; pageSize: number }>();
  @Output() activate           = new EventEmitter<string>();
  @Output() viewDetail         = new EventEmitter<string>();
  @Output() viewClaims         = new EventEmitter<string>();
  @Output() newPolicy          = new EventEmitter<void>();

  readonly displayedColumns = ['policyNumber', 'insured', 'type', 'status', 'insuredAmount', 'actions'];
  readonly dataSource = new MatTableDataSource<Policy>();

  get insuredName(): (p: Policy) => string {
    return (p) => `${p.insured.firstName} ${p.insured.lastName}`;
  }

  onPageEvent(event: PageEvent): void {
    this.pageChange.emit({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }
}
