import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ClaimsCoreService } from '../../../core/service/claims.service';
import { Claim, ClaimFilters, ClaimStatus } from '../../../core/models/claim.model';
import { StatusBadgeComponent } from '../../../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';

@Component({
  selector: 'app-claims-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    StatusLabelPipe,
  ],
  templateUrl: './claims-list.component.html',
  styleUrl: './claims-list.component.css',
})
export class ClaimsListComponent implements OnInit {
  service = inject(ClaimsCoreService);
  router = inject(Router);
  route = inject(ActivatedRoute);

  displayedColumns = ['claimNumber', 'policy', 'description', 'claimAmount', 'status', 'actions'];
  dataSource = new MatTableDataSource<Claim>();

  statusOptions: ClaimStatus[] = ['Registered', 'Approved', 'Rejected', 'Appealed', 'Paid'];
  filters: ClaimFilters = { page: 1, pageSize: 10 };

  ngOnInit() {
    const policyId = this.route.snapshot.queryParamMap.get('policyId');
    if (policyId) this.filters.policyId = policyId;
    this.loadClaims();
  }

  loadClaims() {
    this.service.loading.set(true);
    this.service.getAll(this.filters).subscribe({
      next: (result) => {
        this.dataSource.data = result.items;
        this.service.totalCount.set(result.totalCount);
        this.service.loading.set(false);
      },
      error: () => this.service.loading.set(false),
    });
  }

  onFilterChange() {
    this.filters.page = 1;
    this.loadClaims();
  }

  onPageChange(event: PageEvent) {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadClaims();
  }
}
