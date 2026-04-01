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
import { ClaimsService } from '../services/claims.service';
import { ClaimDto, ClaimFilters, ClaimStatus } from '../models/claim.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { StatusLabelPipe } from '../../../shared/pipes/status-label.pipe';

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
  template: `
    <app-page-header title="Siniestros" />

    <div class="filters">
      <mat-form-field appearance="outline">
        <mat-label>Estado</mat-label>
        <mat-select [(ngModel)]="filters.status" (ngModelChange)="onFilterChange()">
          <mat-option [value]="undefined">Todos</mat-option>
          @for (s of statusOptions; track s) {
            <mat-option [value]="s">{{ s | statusLabel }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
    </div>

    @if (service.loading()) {
      <app-loading-spinner [loading]="true" />
    } @else {
      <mat-table [dataSource]="dataSource">
        <ng-container matColumnDef="claimNumber">
          <mat-header-cell *matHeaderCellDef>Número</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.claimNumber }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="policy">
          <mat-header-cell *matHeaderCellDef>Póliza</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.policyNumber }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="description">
          <mat-header-cell *matHeaderCellDef>Descripción</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.description }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="claimAmount">
          <mat-header-cell *matHeaderCellDef>Monto</mat-header-cell>
          <mat-cell *matCellDef="let row">{{ row.claimAmount | currency:'USD' }}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="status">
          <mat-header-cell *matHeaderCellDef>Estado</mat-header-cell>
          <mat-cell *matCellDef="let row">
            <app-status-badge [status]="row.status" />
          </mat-cell>
        </ng-container>

        <ng-container matColumnDef="actions">
          <mat-header-cell *matHeaderCellDef></mat-header-cell>
          <mat-cell *matCellDef="let row">
            <button mat-icon-button (click)="router.navigate(['/claims', row.id])" aria-label="Ver detalle">
              <mat-icon>visibility</mat-icon>
            </button>
          </mat-cell>
        </ng-container>

        <mat-header-row *matHeaderRowDef="displayedColumns" />
        <mat-row *matRowDef="let row; columns: displayedColumns;" />
      </mat-table>

      <mat-paginator
        [length]="service.totalCount()"
        [pageSize]="filters.pageSize"
        [pageSizeOptions]="[10, 25, 50]"
        (page)="onPageChange($event)"
        showFirstLastButtons />
    }
  `,
  styles: [`
    .filters { display: flex; gap: 16px; margin-bottom: 16px; }
    mat-table { width: 100%; }
  `],
})
export class ClaimsListComponent implements OnInit {
  service = inject(ClaimsService);
  router = inject(Router);
  route = inject(ActivatedRoute);

  displayedColumns = ['claimNumber', 'policy', 'description', 'claimAmount', 'status', 'actions'];
  dataSource = new MatTableDataSource<ClaimDto>();

  statusOptions: ClaimStatus[] = ['Registered', 'Approved', 'Rejected', 'Appealed', 'Paid'];
  filters: ClaimFilters = { page: 1, pageSize: 10 };

  ngOnInit() {
    const policyId = this.route.snapshot.queryParamMap.get('policyId');
    if (policyId) this.filters.policyId = policyId;
    this.loadClaims();
  }

  loadClaims() {
    this.service.loading.set(true);
    this.service.getClaims(this.filters).subscribe({
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
