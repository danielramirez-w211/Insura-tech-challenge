import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ClaimsCoreService } from '../../../core/service/claims.service';
import { Claim, ClaimFilters, ClaimStatus } from '../../../core/models/claim.model';
import { StatusBadgeComponent } from '../../../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../../../shared/pipes/status-label.pipe';
import { AuthService } from '../../../../../core/services/auth.service';

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
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
    MatTooltipModule,
    StatusBadgeComponent,
    PageHeaderComponent,
    LoadingSpinnerComponent,
    StatusLabelPipe,
  ],
  templateUrl: './claims-list.component.html',
  styleUrl: './claims-list.component.css',
})
export class ClaimsListComponent implements OnInit {
  service    = inject(ClaimsCoreService);
  private auth  = inject(AuthService);
  private snack = inject(MatSnackBar);
  router     = inject(Router);
  route      = inject(ActivatedRoute);

  displayedColumns = ['claimNumber', 'policy', 'description', 'claimAmount', 'status', 'actions'];
  dataSource       = new MatTableDataSource<Claim>();

  statusOptions: ClaimStatus[] = ['PendingApproval', 'Registered', 'UnderInvestigation', 'Approved', 'Rejected', 'Appealed', 'Paid'];
  filters: ClaimFilters = { page: 1, pageSize: 10 };

  rejectingId = signal<string | null>(null);
  rejectReason = signal('');

  get isLeader() { return this.auth.role() === 'Leader'; }

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

  approvePending(claim: Claim) {
    this.service.approvePending(claim.id).subscribe({
      next: (updated) => {
        const idx = this.dataSource.data.findIndex(c => c.id === claim.id);
        if (idx >= 0) {
          this.dataSource.data[idx] = updated;
          this.dataSource.data = [...this.dataSource.data];
        }
        this.snack.open('Siniestro aprobado', 'Cerrar', { duration: 3000 });
      },
      error: () => this.snack.open('Error al aprobar el siniestro', 'Cerrar', { duration: 3000 }),
    });
  }

  startReject(claim: Claim) {
    this.rejectingId.set(claim.id);
    this.rejectReason.set('');
  }

  confirmReject(claim: Claim) {
    const reason = this.rejectReason().trim();
    if (!reason) return;
    this.service.rejectPending(claim.id, reason).subscribe({
      next: (updated) => {
        const idx = this.dataSource.data.findIndex(c => c.id === claim.id);
        if (idx >= 0) {
          this.dataSource.data[idx] = updated;
          this.dataSource.data = [...this.dataSource.data];
        }
        this.rejectingId.set(null);
        this.snack.open('Siniestro rechazado', 'Cerrar', { duration: 3000 });
      },
      error: () => this.snack.open('Error al rechazar el siniestro', 'Cerrar', { duration: 3000 }),
    });
  }

  cancelReject() { this.rejectingId.set(null); }
}
